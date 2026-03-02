using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(
        IWarehouseService warehouseService,
        ILogger<WarehouseController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all warehouses
    /// </summary>
    /// <returns>List of all warehouses</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            return Ok(new
            {
                success = true,
                message = "Warehouses retrieved successfully",
                data = warehouses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all warehouses");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving warehouses",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get warehouse by ID
    /// </summary>
    /// <param name="id">Warehouse ID</param>
    /// <returns>Warehouse details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
            
            if (warehouse == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Warehouse not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Warehouse retrieved successfully",
                data = warehouse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warehouse {WarehouseId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving warehouse",
                error = ex.Message
            });
        }
    }
}
