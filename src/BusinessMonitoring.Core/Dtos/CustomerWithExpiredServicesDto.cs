namespace BusinessMonitoring.Core.DTOs;

public class CustomerWithExpiredServicesDto
{
    public string CustomerId { get; set; } = string.Empty;
    public int ExpiredServicesCount { get; set; }
}
