using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/damage-reports")]
[Authorize(Roles = "Admin,Manager,Warehouse Manager,Store Staff,Warehouse Staff")]
public class DamageReportsController : ControllerBase
{
    private readonly IDamageReportService _damageReportService;
    private readonly ILogger<DamageReportsController> _logger;

    public DamageReportsController(
        IDamageReportService damageReportService,
        ILogger<DamageReportsController> logger)
    {
        _damageReportService = damageReportService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/damage-reports - Get all damage reports
    /// Roles: Admin, Manager, Warehouse Manager, Store Staff, Warehouse Staff
    /// </summary>
    /// <returns>List of all damage reports</returns>
    [HttpGet("Get-All-Damage-Reports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllDamageReports()
    {
        try
        {
            var reports = await _damageReportService.GetAllDamageReportsAsync();
            return Ok(new
            {
                success = true,
                message = "Damage reports retrieved successfully",
                data = reports
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all damage reports");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving damage reports",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/damage-reports/{id} - Get damage report by ID
    /// Roles: Admin, Manager, Warehouse Manager, Store Staff, Warehouse Staff
    /// </summary>
    /// <param name="id">Damage report ID</param>
    /// <returns>Damage report details</returns>
    [HttpGet("Get-Damage-Report-By-Id")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDamageReportById(Guid id)
    {
        try
        {
            var report = await _damageReportService.GetDamageReportByIdAsync(id);
            
            if (report == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Damage report with ID {id} not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Damage report retrieved successfully",
                data = report
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting damage report by id: {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving damage report",
                error = ex.Message
            });
        }
    }
}
