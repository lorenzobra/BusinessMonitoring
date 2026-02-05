namespace BusinessMonitoring.Core.Entities;

public class ProcessingError
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string RawData { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? BatchId { get; set; }
}
