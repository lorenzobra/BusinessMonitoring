using BusinessMonitoring.Core.Enums;

namespace BusinessMonitoring.Core.Entities;

public class UploadHistory
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public DateTimeOffset? ProcessingStartedAt { get; set; }
    public DateTimeOffset? ProcessingCompletedAt { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Pending;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? BatchId { get; set; }
}
