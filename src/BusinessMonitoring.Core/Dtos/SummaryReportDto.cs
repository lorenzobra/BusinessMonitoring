namespace BusinessMonitoring.Core.DTOs;

public class SummaryReportDto
{
    public Dictionary<string, int> ActiveServicesByType { get; set; } = new();
    public decimal AverageSpendPerCustomer { get; set; }
    public List<CustomerWithExpiredServicesDto> CustomersWithMultipleExpiredServices { get; set; } = new();
    public List<ExpiringServiceDto> ServicesExpiringIn15Days { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
