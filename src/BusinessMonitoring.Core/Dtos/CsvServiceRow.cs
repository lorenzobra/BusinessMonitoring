using BusinessMonitoring.Core.Enums;

namespace BusinessMonitoring.Core.DTOs;

public class CsvServiceRow
{
    public string CustomerId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTimeOffset ActivationDate { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RowNumber { get; set; }
    public string RawLine { get; set; } = string.Empty;
}
