using BusinessMonitoring.Core.Entities;
using BusinessMonitoring.Core.Interfaces;
using BusinessMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessMonitoring.Infrastructure.Repositories;

public class ProcessingErrorRepository : IProcessingErrorRepository
{
    private readonly ApplicationDbContext _context;

    public ProcessingErrorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogErrorsAsync(
        string fileName,
        List<(int rowNumber, string error, string rawData)> errors,
        Guid? batchId = null)
    {
        var errorEntities = errors.Select(e => new ProcessingError
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            RowNumber = e.rowNumber,
            ErrorMessage = e.error,
            RawData = e.rawData,
            CreatedAt = DateTime.UtcNow,
            BatchId = batchId
        }).ToList();

        await _context.ProcessingErrors.AddRangeAsync(errorEntities);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ProcessingError>> GetErrorsByFileNameAsync(string fileName)
    {
        return await _context.ProcessingErrors
            .Where(e => e.FileName == fileName)
            .OrderBy(e => e.RowNumber)
            .AsNoTracking()
            .ToListAsync();
    }
}