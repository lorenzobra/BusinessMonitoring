using BusinessMonitoring.Core.DTOs;
using BusinessMonitoring.Core.Entities;
using BusinessMonitoring.Core.Enums;
using BusinessMonitoring.Core.Interfaces;
using BusinessMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BusinessMonitoring.Infrastructure.Repositories;
public class ServiceRepository : IServiceRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceRepository> _logger;

    public ServiceRepository(ApplicationDbContext context, ILogger<ServiceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task UpsertServicesAsync(string fileName, List<CsvServiceRow> rows)
    {
        _logger.LogInformation("Upserting {Count} services from file {FileName}", rows.Count, fileName);

        foreach (var row in rows)
        {
            var serviceType = Enum.Parse<ServiceType>(row.ServiceType, true);
            var status = Enum.Parse<ServiceStatus>(row.Status, true);

            // Check if service already exists
            var existing = await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.CustomerId == row.CustomerId &&
                    s.ServiceType == serviceType &&
                    s.ActivationDate == row.ActivationDate);

            if (existing != null)
            {
                // UPDATE
                existing.ExpirationDate = row.ExpirationDate;
                existing.Amount = row.Amount;
                existing.Status = status;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedFromFile = fileName;

                _context.Services.Update(existing);
            }
            else
            {
                // INSERT
                var newService = new Service
                {
                    Id = Guid.NewGuid(),
                    CustomerId = row.CustomerId,
                    ServiceType = serviceType,
                    ActivationDate = row.ActivationDate,
                    ExpirationDate = row.ExpirationDate,
                    Amount = row.Amount,
                    Status = status,
                    CreatedAt = DateTime.UtcNow,
                    ImportedFromFile = fileName
                };

                await _context.Services.AddAsync(newService);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully upserted {Count} services", rows.Count);
    }

    public async Task<List<CustomerExpiredSummary>> GetCustomersWithExpiredServicesAsync(int minExpiredCount)
    {
        return await _context.Services
            .Where(s => s.Status == ServiceStatus.Expired)
            .GroupBy(s => s.CustomerId)
            .Where(g => g.Count() > minExpiredCount)
            .Select(g => new CustomerExpiredSummary
            {
                CustomerId = g.Key,
                ExpiredCount = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<LongRunningService>> GetServicesActiveForYearsAsync(int minYears)
    {
        var thresholdDate = DateTime.UtcNow.AddYears(-minYears);

        return await _context.Services
            .Where(s => s.Status == ServiceStatus.Active && s.ActivationDate < thresholdDate)
            .Select(s => new LongRunningService
            {
                CustomerId = s.CustomerId,
                ServiceType = s.ServiceType.ToString(),
                ActivationDate = s.ActivationDate,
                YearsActive = EF.Functions.DateDiffYear(s.ActivationDate, DateTime.UtcNow)
            })
            .ToListAsync();
    }

    public async Task<SummaryReportDto> GetSummaryReportAsync()
    {
        var report = new SummaryReportDto
        {
            GeneratedAt = DateTime.UtcNow
        };

        //Active services by type
        report.ActiveServicesByType = await _context.Services
            .Where(s => s.Status == ServiceStatus.Active)
            .GroupBy(s => s.ServiceType)
            .Select(g => new { ServiceType = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.ServiceType, x => x.Count);

        //Average spend per customer
        var customerSpends = await _context.Services
            .GroupBy(s => s.CustomerId)
            .Select(g => g.Sum(s => s.Amount))
            .ToListAsync();

        report.AverageSpendPerCustomer = customerSpends.Any()
            ? customerSpends.Average()
            : 0;

        //Customers with more than 1 expired service
        report.CustomersWithMultipleExpiredServices = await _context.Services
            .Where(s => s.Status == ServiceStatus.Expired)
            .GroupBy(s => s.CustomerId)
            .Where(g => g.Count() > 1)
            .Select(g => new CustomerWithExpiredServicesDto
            {
                CustomerId = g.Key,
                ExpiredServicesCount = g.Count()
            })
            .ToListAsync();

        //Services expiring in next 15 days
        var now = DateTimeOffset.UtcNow;
        var futureDate = now.AddDays(15);

        report.ServicesExpiringIn15Days = await _context.Services
            .Where(s => s.Status == ServiceStatus.Active &&
                        s.ExpirationDate >= now &&
                        s.ExpirationDate <= futureDate)
            .Select(s => new ExpiringServiceDto
            {
                CustomerId = s.CustomerId,
                ServiceType = s.ServiceType.ToString(),
                ExpirationDate = s.ExpirationDate,
                DaysUntilExpiration = EF.Functions.DateDiffDay(now, s.ExpirationDate)
            })
            .OrderBy(s => s.ExpirationDate)
            .ToListAsync();

        return report;
    }
}