using BusinessMonitoring.Core.Events;
using BusinessMonitoring.Core.Interfaces;
using MassTransit;
using System.Text;

namespace BusinessMonitoring.Worker.Consumers;

public class UpsellOpportunityConsumer : IConsumer<CsvImportCompletedEvent>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpsellOpportunityConsumer> _logger;
    private const int MinYearsForUpsell = 3;

    public UpsellOpportunityConsumer(
        IServiceRepository serviceRepository,
        IEmailService emailService,
        ILogger<UpsellOpportunityConsumer> logger)
    {
        _serviceRepository = serviceRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CsvImportCompletedEvent> context)
    {
        var @event = context.Message;

        // Skip if import failed
        if (!@event.Success)
        {
            _logger.LogInformation(
                "Skipping upsell check for failed import. BatchId: {BatchId}",
                @event.BatchId
            );
            return;
        }

        _logger.LogInformation(
            "Checking for upsell opportunities. BatchId: {BatchId}",
            @event.BatchId
        );

        try
        {
            // Query for services active for more than threshold years
            var longRunningServices = await _serviceRepository
                .GetServicesActiveForYearsAsync(MinYearsForUpsell);

            if (!longRunningServices.Any())
            {
                _logger.LogInformation(
                    "No upsell opportunities found (services active > {Years} years)",
                    MinYearsForUpsell
                );
                return;
            }

            // Build email body
            var emailBody = "Email body";

            // Send email to marketing team
            await _emailService.SendWithRetryAsync(
                to: "marketing@company.com",
                subject: $"🎯 Upselling Opportunities Detected - {longRunningServices.Count} Customers",
                body: emailBody,
                isHtml: true
            );

            _logger.LogWarning(
                "✅ Upsell email sent to marketing. Opportunities: {Count}, BatchId: {BatchId}",
                longRunningServices.Count, @event.BatchId
            );

            // Optionally publish individual events for analytics/tracking
            foreach (var service in longRunningServices)
            {
                await context.Publish(new UpsellOpportunityDetectedEvent
                {
                    EventId = Guid.NewGuid(),
                    CustomerId = service.CustomerId,
                    ServiceType = service.ServiceType,
                    ActiveForYears = service.YearsActive,
                    ActivationDate = service.ActivationDate,
                    DetectedAt = DateTimeOffset.UtcNow,
                    TriggeredByBatchId = @event.BatchId
                });

                _logger.LogInformation(
                    "💡 Upsell opportunity: Customer {CustomerId}, Service {ServiceType}, Active {Years} years",
                    service.CustomerId, service.ServiceType, service.YearsActive
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking upsell opportunities. BatchId: {BatchId}",
                @event.BatchId
            );
            throw;
        }
    }
}
