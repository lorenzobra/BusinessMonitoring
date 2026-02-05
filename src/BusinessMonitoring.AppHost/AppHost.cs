var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Session)   
    .AddDatabase("BusinessMonitoringDb");

var rabbitMq = builder.AddRabbitMQ("RabbitMQ")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Session);

var sharedPath = Path.Combine(builder.AppHostDirectory, "..", "uploads");
Directory.CreateDirectory(sharedPath);

builder.AddProject<Projects.BusinessMonitoring_Api>("businessmonitoring-api")
    .WithEnvironment("FileStorage__UploadPath", sharedPath)
    .WithReference(sqlServer)
    .WithReference(rabbitMq)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.BusinessMonitoring_Worker>("businessmonitoring-worker")
    .WithEnvironment("FileStorage__UploadPath", sharedPath)
    .WithReference(sqlServer)
    .WithReference(rabbitMq);


builder.Build().Run();
