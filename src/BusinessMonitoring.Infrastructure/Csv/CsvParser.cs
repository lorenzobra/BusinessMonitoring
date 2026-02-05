using BusinessMonitoring.Core.DTOs;
using BusinessMonitoring.Core.Enums;
using BusinessMonitoring.Core.Interfaces;
using BusinessMonitoring.Core.Validators;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace BusinessMonitoring.Infrastructure.Csv;
public class CsvParser : ICsvParser
{
    private readonly ILogger<CsvParser> _logger;
    private readonly CsvServiceRowValidator _validator;

    public CsvParser(ILogger<CsvParser> logger)
    {
        _logger = logger;
        _validator = new CsvServiceRowValidator();
    }

    public async Task<(List<CsvServiceRow> validRows, List<(int rowNumber, string error, string rawData)> invalidRows)>
        ParseAsync(Stream fileStream)
    {
        var validRows = new List<CsvServiceRow>();
        var invalidRows = new List<(int rowNumber, string error, string rawData)>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, config);

        csv.Context.RegisterClassMap<CsvServiceRowMap>();

        int rowNumber = 1; // Header is row 0

        try
        {
            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                rowNumber++;
                var rawLine = csv.Context.Parser?.RawRecord ?? string.Empty;

                try
                {
                    var row = csv.GetRecord<CsvServiceRow>();

                    if (row == null)
                    {
                        invalidRows.Add((rowNumber, "Failed to parse row", rawLine));
                        continue;
                    }

                    row.RowNumber = rowNumber;
                    row.RawLine = rawLine;

                    // Validate
                    var validationResult = await _validator.ValidateAsync(row);

                    if (validationResult.IsValid)
                    {
                        row.Status = row.Status switch
                        {
                            "active" => ServiceStatus.Active.ToString(),
                            "expired" => ServiceStatus.Expired.ToString(),
                            "pending_renewal" => ServiceStatus.PendingRenewal.ToString(),
                            _ => row.Status
                        };
                        row.ServiceType = row.ServiceType switch
                        {
                            "spid" => ServiceType.Spid.ToString(),
                            "pec" => ServiceType.Pec.ToString(),
                            "hosting" => ServiceType.Hosting.ToString(),
                            "fatturazione" => ServiceType.Fatturazione.ToString(),
                            _ => row.ServiceType.ToString()

                        };

                        validRows.Add(row);
                    }
                    else
                    {
                        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                        invalidRows.Add((rowNumber, errors, rawLine));
                        _logger.LogWarning("Row {RowNumber} validation failed: {Errors}", rowNumber, errors);
                    }
                }
                catch (Exception ex)
                {
                    invalidRows.Add((rowNumber, $"Parse error: {ex.Message}", rawLine));
                    _logger.LogWarning(ex, "Failed to parse row {RowNumber}", rowNumber);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error while parsing CSV");
            throw;
        }

        _logger.LogInformation(
            "CSV parsing completed. Valid rows: {ValidCount}, Invalid rows: {InvalidCount}",
            validRows.Count, invalidRows.Count
        );

        return (validRows, invalidRows);
    }
}
public class CsvServiceRowMap : ClassMap<CsvServiceRow>
{
    public CsvServiceRowMap()
    {
        Map(m => m.CustomerId).Name("customer_id");
        Map(m => m.ActivationDate).Name("activation_date").TypeConverter<FlexibleDateTimeOffsetConverter>();
        Map(m => m.ExpirationDate).Name("expiration_date").TypeConverter<FlexibleDateTimeOffsetConverter>();
        Map(m => m.Amount).Name("amount");
        Map(m => m.ServiceType).Name("service_type");
        Map(m => m.Status).Name("status");
    }
}
public class FlexibleDateTimeOffsetConverter : DefaultTypeConverter
{
    private static readonly string[] Formats =
    {
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:sszzz",
        "o"
    };

    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return default(DateTimeOffset);

        // Prova i formati noti
        if (DateTimeOffset.TryParseExact(text, Formats, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var dto))
        {
            return dto;
        }

        throw new CsvHelperException(row.Context, $"Invalid date format: {text}");
    }
}

