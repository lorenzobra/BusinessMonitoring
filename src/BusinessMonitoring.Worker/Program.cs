using BusinessMonitoring.Infrastructure;
using BusinessMonitoring.Worker.Consumers;
using BusinessMonitoring.Worker.Consumers.Definitions;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

// Add Infrastructure (Database, Repositories, Services)
var sqlConnectionString = builder.Configuration.GetConnectionString("BusinessMonitoringDb");
var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMQ");

builder.Services.AddInfrastructure(sqlConnectionString!);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CsvUploadedConsumer>(typeof(CsvUploadedConsumerDefinition));
    x.AddConsumer<ExpiredServicesConsumer>(typeof(ExpiredServicesConsumerDefinition));
    x.AddConsumer<UpsellOpportunityConsumer>(typeof(UpsellOpportunityConsumerDefinition));

    // Configure RabbitMQ using shared configuration
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(rabbitMqConnectionString!, context);
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

Log.Information("Business Monitoring Worker starting...");
Log.Information("Listening for messages on RabbitMQ: {RabbitMQ}", rabbitMqConnectionString);

await app.RunAsync();

Log.Information("Business Monitoring Worker stopped");

