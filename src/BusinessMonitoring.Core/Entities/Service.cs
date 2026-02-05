using BusinessMonitoring.Core.Enums;

namespace BusinessMonitoring.Core.Entities;

public class Service
{
    public Guid Id { get; set; }

    public string CustomerId { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
    public DateTimeOffset ActivationDate { get; set; }

    public DateTimeOffset ExpirationDate { get; set; }
    public decimal Amount { get; set; }
    public ServiceStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string ImportedFromFile { get; set; } = string.Empty;
    public string? UpdatedFromFile { get; set; }


    public bool IsExpired => Status == ServiceStatus.Expired;
    public bool IsExpiringInDays(int days) =>
        Status == ServiceStatus.Active &&
        ExpirationDate <= DateTimeOffset.UtcNow.AddDays(days);
    public int YearsActive =>
        Status == ServiceStatus.Active
            ? (int)(DateTimeOffset.UtcNow - ActivationDate).TotalDays / 365
            : 0;
}