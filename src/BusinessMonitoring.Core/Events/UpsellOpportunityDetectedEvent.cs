namespace BusinessMonitoring.Core.Events;

public class UpsellOpportunityDetectedEvent
{
    public Guid EventId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public int ActiveForYears { get; set; }
    public DateTimeOffset ActivationDate { get; set; }
    public DateTimeOffset DetectedAt { get; set; }
    public Guid TriggeredByBatchId { get; set; }
}