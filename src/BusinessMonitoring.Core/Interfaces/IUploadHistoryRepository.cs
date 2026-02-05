using BusinessMonitoring.Core.Entities;

namespace BusinessMonitoring.Core.Interfaces;

public interface IUploadHistoryRepository
{
    Task<UploadHistory> CreateAsync(UploadHistory history);
    Task UpdateAsync(UploadHistory history);
    Task<UploadHistory?> GetByIdAsync(Guid id);
    Task<UploadHistory?> GetByFileNameAsync(string fileName);
}