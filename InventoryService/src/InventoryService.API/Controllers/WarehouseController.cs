using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("warehouses/{parentId}")]
    public async Task<ActionResult<List<Warehouse>>> GetAllWarehousesByParentId([FromRoute] Guid parentId)
    {
        var warehouses = await _warehouseService.GetAllWarehouseByParentIdAsync(parentId);
        if (warehouses == null || warehouses.Count == 0)
            return NotFound("No warehouse is found with this parent id");
        return Ok(warehouses);
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

    [HttpPost("warehouses")]
    [Authorize(Roles = "Admin,Warehouse Admin")]
    public async Task<ActionResult> AddWarehouse([FromBody] WarehouseCreateRequest request)
    {
        try
        {
            // Validate input
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { success = false, message = "Warehouse name is required" });

            if (request.Capacity <= 0)
                return BadRequest(new { success = false, message = "Capacity must be greater than 0" });

            var validStatuses = new[] { "ACTIVE", "INACTIVE" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { success = false, message = "Status must be ACTIVE or INACTIVE" });

            Warehouse warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Location = request.Location,
                Capacity = request.Capacity,
                Status = request.Status,
                ParentId = request.ParentId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            await _warehouseService.AddWarehouseAsync(warehouse);

            return Ok(new
            {
                success = true,
                message = "Warehouse added successfully",
                data = warehouse.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding warehouse");
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred",
                error = ex.Message
            });
        }
    }

    [HttpPatch("warehouses/{id}")]
    [Authorize(Roles = "Admin,Warehouse Admin")]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] WarehouseUpdateRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Invalid request body" });

        var warehouse = await _warehouseService.GetWarehouseAsync(id);

        if (warehouse == null)
            return NotFound(new { success = false, message = "Warehouse not found" });

        try
        {
            // Validate and update fields
            if (request.Name != null)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(new { success = false, message = "Warehouse name cannot be empty" });
                warehouse.Name = request.Name;
            }

            if (request.Location != null)
                warehouse.Location = request.Location;

            if (request.ParentId != null)
                warehouse.ParentId = request.ParentId;

            if (request.Capacity != null)
            {
                if (request.Capacity <= 0)
                    return BadRequest(new { success = false, message = "Capacity must be greater than 0" });
                warehouse.Capacity = request.Capacity.Value;
            }

            if (request.Status != null)
            {
                var validStatuses = new[] { "ACTIVE", "INACTIVE" };
                if (!validStatuses.Contains(request.Status))
                    return BadRequest(new { success = false, message = "Status must be ACTIVE or INACTIVE" });
                warehouse.Status = request.Status;
            }

            if (request.IsDeleted != null)
                warehouse.IsDeleted = request.IsDeleted.Value;

            await _warehouseService.UpdateWarehouseAsync(warehouse);
            return Ok(new
            {
                success = true,
                message = "Warehouse updated successfully",
                data = warehouse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse {WarehouseId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred",
                error = ex.Message
            });
        }
    }

    [HttpDelete("warehouses/{id}")]
    [Authorize(Roles = "Admin,Warehouse Admin")]
    public async Task<IActionResult> DeleteWarehouse([FromRoute] Guid id)
    {
        var warehouse = await _warehouseService.GetWarehouseAsync(id);

        if (warehouse == null)
            return NotFound(new { success = false, message = "Warehouse not found" });

        try
        {
            await _warehouseService.DeleteWarehouseAsync(id);
            return Ok(new
            {
                success = true,
                message = "Warehouse soft deleted successfully",
                data = id
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot delete warehouse {WarehouseId}: {Reason}", id, ex.Message);
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Warehouse not found for deletion: {WarehouseId}", id);
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse {WarehouseId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred",
                error = ex.Message
            });
        }
    }
}
