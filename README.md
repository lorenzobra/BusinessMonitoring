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
