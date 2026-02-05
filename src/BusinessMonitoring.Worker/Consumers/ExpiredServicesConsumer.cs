using BusinessMonitoring.Core.Events;
using BusinessMonitoring.Core.Interfaces;
using MassTransit;

namespace BusinessMonitoring.Worker.Consumers;

public class ExpiredServicesConsumer : IConsumer<CsvImportCompletedEvent>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<ExpiredServicesConsumer> _logger;
    private const int ExpiredServicesThreshold = 5;

    public ExpiredServicesConsumer(
        IServiceRepository serviceRepository,
        ILogger<ExpiredServicesConsumer> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CsvImportCompletedEvent> context)
    {
        var @event = context.Message;

        // Skip if import failed
        if (!@event.Success)
        {
            _logger.LogInformation(
                "Skipping expired services check for failed import. BatchId: {BatchId}",
                @event.BatchId
            );
            return;
        }

        _logger.LogInformation(
            "Checking for customers with expired services. BatchId: {BatchId}",
            @event.BatchId
        );

        try
        {
            // Query for customers with more than threshold expired services
            var customersWithExpiredServices = await _serviceRepository
                .GetCustomersWithExpiredServicesAsync(ExpiredServicesThreshold);

            if (!customersWithExpiredServices.Any())
            {
                _logger.LogInformation(
                    "No customers found with more than {Threshold} expired services",
                    ExpiredServicesThreshold
                );
                return;
            }

            // Publish alert for each customer
            foreach (var customer in customersWithExpiredServices)
            {
                var alert = new CustomerExpiredServicesAlert
                {
                    EventId = Guid.NewGuid(),
                    CustomerId = customer.CustomerId,
                    ExpiredServicesCount = customer.ExpiredCount,
                    DetectedAt = DateTime.UtcNow,
                    TriggeredByBatchId = @event.BatchId
                };

                // Publish to external queue (consumed by external systems)
                await context.Publish(alert);

                _logger.LogWarning(
                    "⚠️ ALERT: Customer {CustomerId} has {Count} expired services",
                    customer.CustomerId, customer.ExpiredCount
                );
            }

            _logger.LogInformation(
                "Expired services check completed. Alerts sent: {Count}, BatchId: {BatchId}",
                customersWithExpiredServices.Count, @event.BatchId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking expired services. BatchId: {BatchId}",
                @event.BatchId
            );
            throw;
        }
    }
}
