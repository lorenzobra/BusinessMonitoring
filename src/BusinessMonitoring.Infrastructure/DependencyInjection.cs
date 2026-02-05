using BusinessMonitoring.Core.Interfaces;
using BusinessMonitoring.Infrastructure.Csv;
using BusinessMonitoring.Infrastructure.Data;
using BusinessMonitoring.Infrastructure.Repositories;
using BusinessMonitoring.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessMonitoring.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string sqlConnectionString)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(sqlConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IUploadHistoryRepository, UploadHistoryRepository>();
        services.AddScoped<IProcessingErrorRepository, ProcessingErrorRepository>();

        services.AddScoped<ICsvParser, CsvParser>();
        services.AddSingleton<IEmailService, MockEmailService>();

        return services;
    }

    public static void ConfigureRabbitMq(
        this IRabbitMqBusFactoryConfigurator cfg,
        string rabbitMqConnectionString,
        IBusRegistrationContext context)
    {
        cfg.Host(new Uri(rabbitMqConnectionString));

        // Global retry policy with exponential backoff
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 3,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromSeconds(30),
            intervalDelta: TimeSpan.FromSeconds(2)
        ));

        // Configure endpoints automatically based on registered consumers
        cfg.ConfigureEndpoints(context);
    }
}
