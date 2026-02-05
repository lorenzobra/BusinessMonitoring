using MassTransit;

namespace BusinessMonitoring.Worker.Consumers.Definitions;

public class UpsellOpportunityConsumerDefinition : ConsumerDefinition<UpsellOpportunityConsumer>
{
    public UpsellOpportunityConsumerDefinition()
    {
        EndpointName = "csv-import-completed-upsell-check";
        ConcurrentMessageLimit = 8;
    }
}