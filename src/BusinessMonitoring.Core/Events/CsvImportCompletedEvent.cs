namespace BusinessMonitoring.Core.Events;

public class CsvImportCompletedEvent
{
    public Guid EventId { get; set; }
    public Guid BatchId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
