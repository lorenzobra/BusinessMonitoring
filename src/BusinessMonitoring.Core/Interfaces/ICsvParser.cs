using BusinessMonitoring.Core.DTOs;

namespace BusinessMonitoring.Core.Interfaces;

public interface ICsvParser
{
    Task<(List<CsvServiceRow> validRows, List<(int rowNumber, string error, string rawData)> invalidRows)>
        ParseAsync(Stream fileStream);
}