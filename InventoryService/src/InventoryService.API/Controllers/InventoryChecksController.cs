using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Models; // For UserContext
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/inventory-checks")]
[Authorize(Roles = "Admin,Manager,Store Manager,Warehouse Manager,Store Staff,Warehouse Staff")]
public class InventoryChecksController : ControllerBase
{
    private readonly IInventoryCheckService _inventoryCheckService;
    private readonly ILogger<InventoryChecksController> _logger;

    public InventoryChecksController(
        IInventoryCheckService inventoryCheckService,
        ILogger<InventoryChecksController> logger)
    {
        _inventoryCheckService = inventoryCheckService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/inventory-checks - Get all inventory checks
    /// Roles: Admin, Manager, Warehouse Manager, Store Staff, Warehouse Staff
    /// </summary>
    /// <param name="year">Optional year for monthly filtering (must be provided with month)</param>
    /// <param name="month">Optional month for monthly filtering (1-12, must be provided with year)</param>
    /// <returns>List of all inventory checks</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllInventoryChecks([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        try
        {
            if (year.HasValue != month.HasValue)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Both year and month must be provided together"
                });
            }

            if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Year must be between 2000 and 2100"
                });
            }

            if (month.HasValue && (month.Value < 1 || month.Value > 12))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Month must be between 1 and 12"
                });
            }

            var checks = await _inventoryCheckService.GetAllInventoryChecksAsync(year, month);
            return Ok(new
            {
                success = true,
                message = "Inventory checks retrieved successfully",
                data = checks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all inventory checks");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving inventory checks",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/inventory-checks/{id} - Get inventory check by ID
    /// Roles: Admin, Manager, Warehouse Manager, Store Staff, Warehouse Staff
    /// </summary>
    /// <param name="id">Inventory check ID</param>
    /// <returns>Inventory check details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInventoryCheckById(Guid id)
    {
        try
        {
            var check = await _inventoryCheckService.GetInventoryCheckByIdAsync(id);
            
            if (check == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Inventory check with ID {id} not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Inventory check retrieved successfully",
                data = check
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory check by id: {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving inventory check",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// POST /api/inventory-checks - Create a new inventory check session (Step 1)
    /// Roles: Admin, Manager, Warehouse Manager, Store Staff, Warehouse Staff
    /// </summary>
    /// <param name="dto">Inventory check creation data</param>
    /// <returns>Created inventory check</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Store Manager,Warehouse Manager,Store Staff,Warehouse Staff")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateInventoryCheck([FromBody] CreateInventoryCheckDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var userContext = GetUserContext();
            var check = await _inventoryCheckService.CreateInventoryCheckAsync(dto, userContext);
            
            return CreatedAtAction(
                nameof(GetInventoryCheckById),
                new { id = check.Id },
                new
                {
                    success = true,
                    message = $"Inventory check {check.CheckNumber} created successfully",
                    data = check
                });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authorization failed when creating inventory check");
            return StatusCode(403, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Location not found when creating inventory check");
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating inventory check");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating inventory check");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory check");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating inventory check",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// PUT /api/inventory-checks/{id}/submit - Submit inventory check results (Step 2)
    /// Roles: Admin, Manager, Warehouse Manager, Store Staff
    /// </summary>
    /// <param name="id">Inventory check ID</param>
    /// <param name="dto">Inventory check submission data</param>
    /// <returns>Updated inventory check</returns>
    [HttpPut("{id:guid}/submit")]
    [Authorize(Roles = "Admin,Manager,Store Manager,Warehouse Manager,Store Staff,Warehouse Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitInventoryCheck(Guid id, [FromBody] SubmitInventoryCheckDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var userContext = GetUserContext();
            var check = await _inventoryCheckService.SubmitInventoryCheckAsync(id, dto, userContext);
            
            return Ok(new
            {
                success = true,
                message = $"Inventory check {check.CheckNumber} submitted successfully with {check.TotalDiscrepancies} discrepancies",
                data = check
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authorization failed when submitting inventory check");
            return StatusCode(403, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Inventory check or product not found");
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when submitting inventory check");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting inventory check {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while submitting inventory check",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// POST /api/inventory-checks/{id}/reconcile - Reconcile differences (Step 3)
    /// Roles: Admin, Manager, Warehouse Manager
    /// </summary>
    /// <param name="id">Inventory check ID</param>
    /// <returns>List of discrepancies</returns>
    [HttpPost("{id:guid}/reconcile")]
    [Authorize(Roles = "Admin,Manager,Store Manager,Warehouse Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReconcileInventoryCheck(Guid id)
    {
        try
        {
            var userContext = GetUserContext();
            var discrepancies = await _inventoryCheckService.ReconcileInventoryCheckAsync(id, userContext);
            
            return Ok(new
            {
                success = true,
                message = $"Found {discrepancies.Count()} discrepancies",
                data = discrepancies
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authorization failed when reconciling inventory check");
            return StatusCode(403, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Inventory check not found");
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when reconciling inventory check");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling inventory check {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while reconciling inventory check",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// PUT /api/inventory-checks/{id}/approve - Approve inventory check (Step 4)
    /// Roles: Admin, Manager, Warehouse Manager
    /// </summary>
    /// <param name="id">Inventory check ID</param>
    /// <param name="dto">Approval data</param>
    /// <returns>Approved inventory check</returns>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager,Store Manager,Warehouse Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveInventoryCheck(Guid id, [FromBody] ApproveInventoryCheckDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var userContext = GetUserContext();
            var check = await _inventoryCheckService.ApproveInventoryCheckAsync(id, dto, userContext);
            
            return Ok(new
            {
                success = true,
                message = $"Inventory check {check.CheckNumber} approved successfully",
                data = check
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authorization failed when approving inventory check");
            return StatusCode(403, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Inventory check not found");
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when approving inventory check");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving inventory check {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while approving inventory check",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// PUT /api/inventory-checks/{id}/adjust - Adjust inventory based on check results (Step 5)
    /// Roles: Admin, Manager, Warehouse Manager
    /// </summary>
    /// <param name="id">Inventory check ID</param>
    /// <param name="dto">Adjustment data</param>
    /// <returns>Updated inventory check</returns>
    [HttpPut("{id:guid}/adjust")]
    [Authorize(Roles = "Admin,Manager,Store Manager,Warehouse Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdjustInventory(Guid id, [FromBody] AdjustInventoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var userContext = GetUserContext();
            var check = await _inventoryCheckService.AdjustInventoryAsync(id, dto, userContext);
            
            return Ok(new
            {
                success = true,
                message = $"Inventory adjusted successfully for check {check.CheckNumber}",
                data = check
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authorization failed when adjusting inventory");
            return StatusCode(403, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Inventory check or inventory record not found");
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when adjusting inventory");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting inventory for check {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while adjusting inventory",
                error = ex.Message
            });
        }
    }

    // =====================================================
    // Helper Method: Extract UserContext from JWT Claims
    // =====================================================
    private UserContext GetUserContext()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException($"{ErrorCodes.MissingClaim}: User ID claim not found");
        
        if (!Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException($"{ErrorCodes.InvalidInput}: Invalid User ID format");
        }
        
        var role = User.FindFirst(ClaimTypes.Role)?.Value 
            ?? throw new UnauthorizedAccessException($"{ErrorCodes.MissingClaim}: Role claim not found");

        return new UserContext
        {
            UserId = userId,
            Role = role
        };
    }
}
