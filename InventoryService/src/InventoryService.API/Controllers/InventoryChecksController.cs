using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/inventory-checks")]
[Authorize(Roles = "Admin,Manager,Warehouse Manager,Store Staff,Warehouse Staff")]
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
    /// <returns>List of all inventory checks</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllInventoryChecks()
    {
        try
        {
            var checks = await _inventoryCheckService.GetAllInventoryChecksAsync();
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
}
