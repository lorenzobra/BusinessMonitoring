using MassTransit;

namespace BusinessMonitoring.Worker.Consumers.Definitions;

public class CsvUploadedConsumerDefinition : ConsumerDefinition<CsvUploadedConsumer>
{
    public CsvUploadedConsumerDefinition()
    {
        // Override endpoint name if needed
        EndpointName = "csv-uploaded";

        // Concurrent message limit
        ConcurrentMessageLimit = 4;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<CsvUploadedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // Consumer-specific retry policy (overrides global)
        endpointConfigurator.UseMessageRetry(r => r.Immediate(2));
    }
}