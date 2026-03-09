using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly IWarehouseSlotService _slotService;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(
        IWarehouseService warehouseService,
        IWarehouseSlotService slotService,
        ILogger<WarehouseController> logger)
    {
        _warehouseService = warehouseService;
        _slotService = slotService;
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

    [HttpPost("warehouses")]
    public async Task<ActionResult> AddWarehouse([FromBody] WarehouseDto warehouseDto)
    {
        try
        {
            Warehouse warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = warehouseDto.Name,
                Location = warehouseDto.Location,
                Capacity = warehouseDto.Capacity,
                Status = warehouseDto.Status,
                IsDeleted = warehouseDto.IsDeleted,
                CreatedAt = warehouseDto.CreatedAt,
                CreatedBy = warehouseDto.CreatedBy
            };

            await _warehouseService.AddWarehouseAsync(warehouse);

            return Ok("Warehouse added successfully");
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occured");
        }
    }

    [HttpPatch("warehouses/{id}")]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] WarehouseUpdateRequest request)
    {
        if (request == null)
            return BadRequest("Invalid request body");

        var warehouse = await _warehouseService.GetWarehouseAsync(id);

        if (warehouse == null)
            return NotFound("Warehouse not found");

        if (request.Name != null)
            warehouse.Name = request.Name;

        if (request.Location != null)
            warehouse.Location = request.Location;

        if (request.Capacity.HasValue)
            warehouse.Capacity = request.Capacity.Value;

        if (request.Status != null)
            warehouse.Status = request.Status;

        try
        {
            await _warehouseService.UpdateWarehouseAsync(warehouse);
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal server error occurred");
        }
    }

    [HttpDelete("warehouses/{id}")]
    public async Task<IActionResult> DeleteWarehouse([FromRoute] Guid id)
    {
        await _warehouseService.DeleteWarehouseAsync(id);
        return Ok("Warehouse deleted successfully");
    }

    [HttpGet("warehouses/{warehouseId}/slots")]
    public async Task<ActionResult<List<WarehouseSlot>>> GetWarehouseSlots(Guid warehouseId)
    {
        try
        {
            var slots = await _warehouseService.GetWarehouseSlotById(warehouseId);
            if (slots == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Warehouse not found or no slots available"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Warehouse slots retrieved successfully",
                data = slots
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warehouse slots for warehouse {WarehouseId}", warehouseId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving warehouse slots",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new slot in a warehouse
    /// </summary>
    /// <param name="warehouseId">Warehouse ID</param>
    /// <param name="request">Slot data</param>
    /// <returns>Created slot</returns>
    [HttpPost("{warehouseId}/slots")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSlot(Guid warehouseId, [FromBody] CreateSlotRequestDto request)
    {
        try
        {
            var slot = await _slotService.CreateSlotAsync(warehouseId, request);

            _logger.LogInformation("Slot {SlotCode} created in warehouse {WarehouseId}", request.SlotCode, warehouseId);

            return CreatedAtAction(
                nameof(SlotController.GetById),
                "Slot",
                new { id = slot.Id },
                new { success = true, message = "Slot created successfully", data = slot });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Create slot failed for warehouse {WarehouseId}: {Message}", warehouseId, ex.Message);
            var status = ex.Message.Contains("not found") ? 404 : 400;
            return StatusCode(status, new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating slot in warehouse {WarehouseId}", warehouseId);
            return StatusCode(500, new { success = false, message = "An error occurred while creating slot" });
        }
    }
}
