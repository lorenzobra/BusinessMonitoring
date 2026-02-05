using BusinessMonitoring.Core.Entities;

namespace BusinessMonitoring.Core.Interfaces;

public interface IProcessingErrorRepository
{
    Task LogErrorsAsync(string fileName, List<(int rowNumber, string error, string rawData)> errors, Guid? batchId = null);
    Task<List<ProcessingError>> GetErrorsByFileNameAsync(string fileName);
}