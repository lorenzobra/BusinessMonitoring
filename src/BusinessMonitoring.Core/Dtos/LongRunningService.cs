namespace BusinessMonitoring.Core.DTOs;

public class LongRunningService
{
    public string CustomerId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTimeOffset ActivationDate { get; set; }
    public int YearsActive { get; set; }
}