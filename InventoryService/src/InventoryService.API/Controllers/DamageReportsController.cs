using InventoryService.Application.Interfaces;
using InventoryService.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/damage-reports")]
[Authorize(Roles = "Admin,Warehouse Manager,Warehouse Admin,Warehouse Staff,Store Manager,Store Staff")]
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
    /// Get current user ID from JWT token claims
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Unable to extract user ID from token");
        }
        return userId;
    }

    /// <summary>
    /// GET /api/damage-reports - Get all damage reports
    /// Roles: Admin, Warehouse Manager, Warehouse Admin, Warehouse Staff
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
    /// Roles: Admin, Warehouse Manager, Warehouse Admin, Warehouse Staff
    /// </summary>
    /// <param name="id">Damage report ID</param>
    /// <returns>Damage report details</returns>
    [HttpGet("Get-Damage-Report-By-Id")]
    [Authorize(Roles = "Admin,Warehouse Manager,Store Manager,Warehouse Admin")]
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

    /// <summary>
    /// POST /api/damage-reports - Create new damage report with photo upload
    /// ReportedBy is automatically set from current user
    /// </summary>
    /// <param name="request">Damage report creation data with photo files</param>
    /// <returns>Created damage report</returns>
    [HttpPost("Create-Damage-Report")]
    [DisableRequestSizeLimit]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDamageReport([FromForm] CreateDamageReportRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var currentUserId = GetCurrentUserId();
            var damageReport = await _damageReportService.CreateDamageReportAsync(request, currentUserId);

            return CreatedAtAction(
                nameof(GetDamageReportById),
                new { id = damageReport.Id },
                new
                {
                    success = true,
                    message = "Damage report created successfully",
                    data = damageReport
                }
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid damage report data");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating damage report");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating damage report",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// PUT /api/damage-reports/{id}/approve - Approve damage report
    /// ApprovedBy is automatically set from current user
    /// Requires role: Admin, Warehouse Manager, Store Manager, or Warehouse Admin
    /// </summary>
    /// <param name="id">Damage report ID</param>
    /// <returns>Updated damage report</returns>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,Warehouse Manager,Store Manager,Warehouse Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveDamageReport(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var damageReport = await _damageReportService.ApproveDamageReportAsync(id, currentUserId);

            return Ok(new
            {
                success = true,
                message = "Damage report approved successfully",
                data = damageReport
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Damage report not found: {Id}", id);
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation on damage report {Id}", id);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid approval data");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving damage report {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while approving damage report",
                error = ex.Message
            });
        }
    }
}
