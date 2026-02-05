using BusinessMonitoring.Core.Entities;
using BusinessMonitoring.Core.Interfaces;
using BusinessMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessMonitoring.Infrastructure.Repositories;

public class UploadHistoryRepository : IUploadHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public UploadHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UploadHistory> CreateAsync(UploadHistory history)
    {
        await _context.UploadHistories.AddAsync(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task UpdateAsync(UploadHistory history)
    {
        _context.UploadHistories.Update(history);
        await _context.SaveChangesAsync();
    }

    public async Task<UploadHistory?> GetByIdAsync(Guid id)
    {
        return await _context.UploadHistories.FindAsync(id);
    }

    public async Task<UploadHistory?> GetByFileNameAsync(string fileName)
    {
        return await _context.UploadHistories
            .Where(u => u.FileName == fileName)
            .OrderByDescending(u => u.UploadedAt)
            .FirstOrDefaultAsync();
    }
}