using MassTransit;

namespace BusinessMonitoring.Core.Events;

[EntityName("alerts.customer_expired")]
public class CustomerExpiredServicesAlert
{
    public Guid EventId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public int ExpiredServicesCount { get; set; }
    public DateTime DetectedAt { get; set; }
    public Guid TriggeredByBatchId { get; set; }
}
