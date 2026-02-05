using BusinessMonitoring.Core.Enums;
using BusinessMonitoring.Core.Events;
using BusinessMonitoring.Core.Interfaces;
using MassTransit;

namespace BusinessMonitoring.Worker.Consumers;

public class CsvUploadedConsumer : IConsumer<CsvUploadedEvent>
{
    private readonly ICsvParser _csvParser;
    private readonly IServiceRepository _serviceRepository;
    private readonly IProcessingErrorRepository _errorRepository;
    private readonly IUploadHistoryRepository _uploadHistoryRepository;
    private readonly ILogger<CsvUploadedConsumer> _logger;

    public CsvUploadedConsumer(
        ICsvParser csvParser,
        IServiceRepository serviceRepository,
        IProcessingErrorRepository errorRepository,
        IUploadHistoryRepository uploadHistoryRepository,
        ILogger<CsvUploadedConsumer> logger)
    {
        _csvParser = csvParser;
        _serviceRepository = serviceRepository;
        _errorRepository = errorRepository;
        _uploadHistoryRepository = uploadHistoryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CsvUploadedEvent> context)
    {
        var @event = context.Message;
        var batchId = Guid.NewGuid();

        _logger.LogInformation(
            "Processing CSV file: {FileName}, BatchId: {BatchId}",
            @event.FileName, batchId
        );

        try
        {
            // Update upload history status
            var uploadHistory = await _uploadHistoryRepository.GetByFileNameAsync(@event.FileName);
            if (uploadHistory != null)
            {
                uploadHistory.ProcessingStatus = ProcessingStatus.Processing;
                uploadHistory.ProcessingStartedAt = DateTime.UtcNow;
                uploadHistory.BatchId = batchId;
                await _uploadHistoryRepository.UpdateAsync(uploadHistory);
            }

            // Parse CSV file
            await using var fileStream = File.OpenRead(@event.FilePath);
            var (validRows, invalidRows) = await _csvParser.ParseAsync(fileStream);

            _logger.LogInformation(
                "CSV parsed. Valid: {ValidCount}, Invalid: {InvalidCount}",
                validRows.Count, invalidRows.Count
            );

            // Log invalid rows
            if (invalidRows.Any())
            {
                await _errorRepository.LogErrorsAsync(@event.FileName, invalidRows, batchId);
                _logger.LogWarning(
                    "Logged {Count} validation errors for file {FileName}",
                    invalidRows.Count, @event.FileName
                );
            }

            // Upsert valid rows into database
            if (validRows.Any())
            {
                await _serviceRepository.UpsertServicesAsync(@event.FileName, validRows);
                _logger.LogInformation(
                    "Upserted {Count} services from file {FileName}",
                    validRows.Count, @event.FileName
                );
            }

            // Update upload history - completed
            if (uploadHistory != null)
            {
                uploadHistory.ProcessingStatus = ProcessingStatus.Completed;
                uploadHistory.ProcessingCompletedAt = DateTime.UtcNow;
                uploadHistory.TotalRows = validRows.Count + invalidRows.Count;
                uploadHistory.ValidRows = validRows.Count;
                uploadHistory.InvalidRows = invalidRows.Count;
                await _uploadHistoryRepository.UpdateAsync(uploadHistory);
            }

            // Publish completion event
            await context.Publish(new CsvImportCompletedEvent
            {
                EventId = Guid.NewGuid(),
                BatchId = batchId,
                FileName = @event.FileName,
                CompletedAt = DateTime.UtcNow,
                TotalRows = validRows.Count + invalidRows.Count,
                ValidRows = validRows.Count,
                InvalidRows = invalidRows.Count,
                Success = true
            });

            _logger.LogInformation(
                "CSV import completed successfully. BatchId: {BatchId}, File: {FileName}",
                batchId, @event.FileName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing CSV file: {FileName}, BatchId: {BatchId}",
                @event.FileName, batchId
            );

            // Update upload history - failed
            var uploadHistory = await _uploadHistoryRepository.GetByFileNameAsync(@event.FileName);
            if (uploadHistory != null)
            {
                uploadHistory.ProcessingStatus = ProcessingStatus.Failed;
                uploadHistory.ProcessingCompletedAt = DateTime.UtcNow;
                uploadHistory.ErrorMessage = ex.Message;
                await _uploadHistoryRepository.UpdateAsync(uploadHistory);
            }

            // Publish failure event
            await context.Publish(new CsvImportCompletedEvent
            {
                EventId = Guid.NewGuid(),
                BatchId = batchId,
                FileName = @event.FileName,
                CompletedAt = DateTime.UtcNow,
                Success = false,
                ErrorMessage = ex.Message
            });

            throw; // Rethrow to trigger MassTransit retry
        }
    }
}