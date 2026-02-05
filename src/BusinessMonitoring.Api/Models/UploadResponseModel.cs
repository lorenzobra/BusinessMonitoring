namespace BusinessMonitoring.Api.Models;

public class UploadResponseModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public Guid UploadId { get; set; }
    public DateTime UploadedAt { get; set; }
}
