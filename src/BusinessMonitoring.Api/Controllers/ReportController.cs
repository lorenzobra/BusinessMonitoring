using BusinessMonitoring.Api.Models;
using BusinessMonitoring.Core.DTOs;
using BusinessMonitoring.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessMonitoring.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IServiceRepository serviceRepository, ILogger<ReportController> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(SummaryReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            _logger.LogInformation("Generating summary report");

            var report = await _serviceRepository.GetSummaryReportAsync();

            _logger.LogInformation(
                "Summary report generated: {ActiveServices} active service types, {ExpiredCustomers} customers with expired services",
                report.ActiveServicesByType.Count,
                report.CustomersWithMultipleExpiredServices.Count
            );

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary report");
            return StatusCode(500, new ErrorResponseModel
            {
                Error = "Report generation failed",
                Details = "An error occurred while generating the report"
            });
        }
    }

    [HttpGet("expired-services")]
    [ProducesResponseType(typeof(List<CustomerExpiredSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomersWithExpiredServices([FromQuery] int minCount = 1)
    {
        try
        {
            var customers = await _serviceRepository.GetCustomersWithExpiredServicesAsync(minCount);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customers with expired services");
            return StatusCode(500, new ErrorResponseModel
            {
                Error = "Query failed",
                Details = "An error occurred while fetching expired services data"
            });
        }
    }

    [HttpGet("upsell-opportunities")]
    [ProducesResponseType(typeof(List<LongRunningService>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpsellOpportunities([FromQuery] int minYears = 3)
    {
        try
        {
            var services = await _serviceRepository.GetServicesActiveForYearsAsync(minYears);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching upsell opportunities");
            return StatusCode(500, new ErrorResponseModel
            {
                Error = "Query failed",
                Details = "An error occurred while fetching upsell opportunities"
            });
        }
    }
}
