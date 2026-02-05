using BusinessMonitoring.Core.DTOs;

namespace BusinessMonitoring.Core.Interfaces;

public interface IServiceRepository
{
    Task UpsertServicesAsync(string fileName, List<CsvServiceRow> rows);

    Task<List<CustomerExpiredSummary>> GetCustomersWithExpiredServicesAsync(int minExpiredCount);

    Task<List<LongRunningService>> GetServicesActiveForYearsAsync(int minYears);

    Task<SummaryReportDto> GetSummaryReportAsync();
}
