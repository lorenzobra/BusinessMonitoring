# BusinessMonitoring

Sistema di monitoraggio business con architettura distribuita basata su .NET Aspire.

## Prerequisiti

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) o superiore
- [Docker Desktop](https://www.docker.com/products/docker-desktop) installato e in esecuzione
- [.NET Aspire Workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

## Architettura

Il progetto è composto da:

- **BusinessMonitoring.Api**: API REST per l'interfaccia utente
- **BusinessMonitoring.Worker**: Worker in background per l'elaborazione asincrona
- **SQL Server**: Database per la persistenza dei dati
- **RabbitMQ**: Message broker per la comunicazione tra servizi
- **Shared Storage**: Directory condivisa per i file caricati

## Avvio del Progetto

1. **Clona il repository**
2. **Assicurati che Docker sia in esecuzione**
   
   Verifica che Docker Desktop sia avviato e funzionante.
   
3. **Avvia l'applicazione**
   
   Dalla directory "src" del progetto (dove si trova il file BusinessMonitoring.sln):
```bash
   dotnet run --project BusinessMonitoring.AppHost
```
4. **Accedi alla Dashboard Aspire**
 
   Dopo l'avvio la console mostrerà l'indirizzo a cui trovare la dashboard di aspire.
   
6. **Accedi all'API**
 
    L'API con cui autenticarsi, caricare il csv e richiedere i report, sarà disponibile all'indirizzo mostrato nel dashboard Aspire (https://localhost:7052/swagger)

## 🔐 Test delle API

Questa sezione descrive i passaggi necessari per autenticarsi e testare gli endpoint esposti dal servizio.

### 1. Effettuare il login

Per prima cosa è necessario ottenere un token JWT tramite l’endpoint di autenticazione.

**Endpoint:** `POST /api/Auth/login`  
**Credenziali di test:**

```json
{
  "username": "admin",
  "password": "admin"
}
```
La risposta conterrà un token JWT nel campo token.

### 2. Autorizzarsi in Swagger

Una volta ottenuto il token:
1.  Cliccare sul pulsante Authorize in alto a destra.
2.  Inserire il token nel formato:
```
bearer {token}
```
3.  Confermare con Authorize

Da questo momento tutte le chiamate agli endpoint protetti verranno eseguite con il token JWT.

### 3. Caricare un CSV

Nel repo nella cartella SampleData sono disponibili 2 file csv di esempio 

Tramite l'endpoint POST api/csv/upload è possibile caricare il file che verrà elaborato dal sistema.

### 4. Monitorare l'elaborazione

Dopo aver caricato un file, tramite la dashboard di Aspire è possibile visualizzare i log strutturati delle applicazioni.



# Flusso di Elaborazione CSV

## 1. Upload File
**Endpoint:** `POST /api/csv/upload`

L'utente carica un file CSV tramite API. Il controller:
- Valida il file (formato .csv, max 10MB)
- Salva il file su filesystem
- Crea record in `UploadHistory` (stato: `Pending`)
- Pubblica evento `CsvUploadedEvent`
- Risponde immediatamente al client con `uploadId`

**Evento generato:** `CsvUploadedEvent`
```json
{
  "fileName": "abc123_data.csv",
  "filePath": "/uploads/abc123_data.csv",
  "uploadedBy": "user@example.com"
}
```

---

## 2. Elaborazione Asincrona
**Consumer:** `CsvUploadedConsumer`

Il worker riceve l'evento e avvia l'elaborazione:
- Aggiorna `UploadHistory` (stato: `Processing`)
- Legge il file da filesystem
- Parsing del CSV con `CsvHelper`
- Validazione righe con `FluentValidation`
- Separa righe valide da quelle invalide

---

## 3. Salvataggio Risultati

Il consumer persiste i dati elaborati:
- **Righe valide** → Upsert in tabella `Services`
- **Righe invalide** → Log in tabella `ProcessingErrors`
- Aggiorna `UploadHistory` (stato: `Completed`, contatori righe)

---

## 4. Notifica Completamento
**Evento generato:** `CsvImportCompletedEvent`

Il consumer pubblica l'evento di completamento:
```json
{
  "batchId": "def456",
  "fileName": "abc123_data.csv",
  "totalRows": 1000,
  "validRows": 980,
  "invalidRows": 20,
  "success": true
}
```

**Consumer che reagiscono all'evento:**

### `UpsellOpportunityConsumer`
Identifica opportunità di upselling analizzando servizi attivi da oltre 3 anni:
- Interroga il database per servizi longevi
- Invia email al team marketing con lista clienti
- Pubblica eventi `UpsellOpportunityDetectedEvent` per tracking analytics

### `ExpiredServicesConsumer`
Monitora clienti con molti servizi scaduti (soglia: 5+):
- Identifica clienti a rischio churn
- Pubblica alert `CustomerExpiredServicesAlert` per sistemi esterni
- Notifiche utilizzabili da CRM o sistemi di retention

## Eventi del Sistema

### `CsvUploadedEvent`
- **Publisher:** `CsvController` (API)
- **Consumer:** `CsvUploadedConsumer` (Worker)
- **Scopo:** Avviare elaborazione asincrona del file

### `CsvImportCompletedEvent`
- **Publisher:** `CsvUploadedConsumer` (Worker)
- **Consumer:** `UpsellOpportunityConsumer`, `ExpiredServicesConsumer`
- **Scopo:** Notificare completamento e triggerare analisi business

### `UpsellOpportunityDetectedEvent`
- **Publisher:** `UpsellOpportunityConsumer` (Worker)
- **Consumer:** Sistemi di analytics/tracking (esterni)
- **Scopo:** Tracciare singole opportunità di upselling

### `CustomerExpiredServicesAlert`
- **Publisher:** `ExpiredServicesConsumer` (Worker)
- **Consumer:** CRM, sistemi di retention (esterni)
- **Scopo:** Alerting clienti a rischio churn
- **Routing:** Configurato con `[EntityName("alerts.customer_expired")]` per instradamento dedicato


## Servizi Esterni

Durante l'avvio, verranno automaticamente creati e configurati i seguenti container Docker:

- **SQL Server**: Database principale
- **RabbitMQ**: Message broker con management plugin (accessibile su `http://localhost:65357/`)
  - Username: `guest`
  - Password: visualizza i dettagli del container dalla dashboard aspire e copiala dalle variabili di ambiente
 
### Ciclo di vita dei container

I container SQL Server e RabbitMQ sono configurati con `ContainerLifetime.Session`, il che significa che:
- Vengono avviati all'inizio della sessione
- Vengono terminati tra riavvii dell'applicazione
