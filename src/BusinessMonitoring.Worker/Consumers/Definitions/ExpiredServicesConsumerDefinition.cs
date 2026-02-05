using MassTransit;

namespace BusinessMonitoring.Worker.Consumers.Definitions;

public class ExpiredServicesConsumerDefinition : ConsumerDefinition<ExpiredServicesConsumer>
{
    public ExpiredServicesConsumerDefinition()
    {
        EndpointName = "csv-import-completed-expired-check";
        ConcurrentMessageLimit = 8; // Can process multiple imports in parallel
    }
}
