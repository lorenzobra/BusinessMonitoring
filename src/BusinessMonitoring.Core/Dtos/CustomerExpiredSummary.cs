namespace BusinessMonitoring.Core.DTOs;

public class CustomerExpiredSummary
{
    public string CustomerId { get; set; } = string.Empty;
    public int ExpiredCount { get; set; }
}
