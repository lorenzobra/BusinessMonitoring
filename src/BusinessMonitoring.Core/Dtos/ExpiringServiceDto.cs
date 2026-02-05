namespace BusinessMonitoring.Core.DTOs;

public class ExpiringServiceDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTimeOffset ExpirationDate { get; set; }
    public int DaysUntilExpiration { get; set; }
}
