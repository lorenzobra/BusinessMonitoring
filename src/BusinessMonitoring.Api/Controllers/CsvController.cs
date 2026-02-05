using BusinessMonitoring.Api.Models;
using BusinessMonitoring.Core.Entities;
using BusinessMonitoring.Core.Enums;
using BusinessMonitoring.Core.Events;
using BusinessMonitoring.Core.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessMonitoring.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CsvController : ControllerBase
{
    private readonly IBus _bus;
    private readonly IUploadHistoryRepository _uploadHistoryRepository;
    private readonly ILogger<CsvController> _logger;
    private readonly string _uploadPath;

    public CsvController(
        IBus bus,
        IUploadHistoryRepository uploadHistoryRepository,
        ILogger<CsvController> logger,
        IConfiguration configuration)
    {
        _bus = bus;
        _uploadHistoryRepository = uploadHistoryRepository;
        _logger = logger;
        _uploadPath = configuration["FileStorage:UploadPath"] ?? Path.Combine(Path.GetTempPath(), "BusinessMonitoring", "uploads");

        // Ensure upload directory exists
        Directory.CreateDirectory(_uploadPath);
    }

    /// <summary>
    /// Upload a CSV file for processing
    /// </summary>
    /// <param name="file">CSV file containing customer service data</param>
    /// <returns>Upload confirmation with tracking ID</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponseModel
            {
                Error = "No file uploaded",
                Details = "Please provide a valid CSV file"
            });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponseModel
            {
                Error = "Invalid file type",
                Details = "Only CSV files are accepted"
            });
        }

        if (file.Length > 10_000_000) // 10MB
        {
            return BadRequest(new ErrorResponseModel
            {
                Error = "File too large",
                Details = "Maximum file size is 10MB"
            });
        }

        try
        {
            var uploadId = Guid.NewGuid();
            var fileName = $"{uploadId}_{file.FileName}";
            var filePath = Path.Combine(_uploadPath, fileName);
            var username = User.Identity?.Name ?? "anonymous";

            // Save file to disk
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation(
                "File uploaded: {FileName} by {Username}, Size: {Size} bytes",
                fileName, username, file.Length
            );

            // Create upload history record
            var uploadHistory = new UploadHistory
            {
                Id = uploadId,
                FileName = fileName,
                UploadedBy = username,
                UploadedAt = DateTime.UtcNow,
                ProcessingStatus = ProcessingStatus.Pending,
            };

            await _uploadHistoryRepository.CreateAsync(uploadHistory);

            // Publish event to message bus for async processing
            await _bus.Publish(new CsvUploadedEvent
            {
                EventId = Guid.NewGuid(),
                FileName = fileName,
                FilePath = filePath,
                UploadedBy = username,
                UploadedAt = DateTime.UtcNow,
                FileSizeBytes = file.Length
            });

            _logger.LogInformation("CSV upload event published for file: {FileName}", fileName);

            return Ok(new UploadResponseModel
            {
                Success = true,
                Message = "File uploaded successfully and queued for processing",
                FileName = fileName,
                UploadId = uploadId,
                UploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            return StatusCode(500, new ErrorResponseModel
            {
                Error = "Upload failed",
                Details = "An error occurred while uploading the file"
            });
        }
    }
}