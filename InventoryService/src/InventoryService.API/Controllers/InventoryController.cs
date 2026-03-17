using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryService inventoryService,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all inventories
    /// </summary>
    /// <returns>List of all inventories</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync();
            return Ok(new
            {
                success = true,
                message = "Inventories retrieved successfully",
                data = inventories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all inventories");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving inventories",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get inventory by ID
    /// </summary>
    /// <param name="id">Inventory ID</param>
    /// <returns>Inventory details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            
            if (inventory == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Inventory not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Inventory retrieved successfully",
                data = inventory
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory {InventoryId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving inventory",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get inventories by location (warehouse or store)
    /// </summary>
    /// <param name="locationType">Location type (WAREHOUSE or STORE)</param>
    /// <param name="locationId">Location ID</param>
    /// <returns>List of inventories for the location</returns>
    [HttpGet("location/{locationType}/{locationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByLocation(string locationType, Guid locationId)
    {
        try
        {
            var inventories = await _inventoryService.GetInventoriesByLocationAsync(locationType, locationId);
            return Ok(new
            {
                success = true,
                message = $"Inventories for {locationType} {locationId} retrieved successfully",
                data = inventories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventories for {LocationType} {LocationId}", locationType, locationId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving inventories",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get inventories by product ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of inventories for the product</returns>
    [HttpGet("product/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        try
        {
            var inventories = await _inventoryService.GetInventoriesByProductAsync(productId);
            return Ok(new
            {
                success = true,
                message = $"Inventories for product {productId} retrieved successfully",
                data = inventories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventories for product {ProductId}", productId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving inventories",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Check if inventory exists for a product at a location. If not, create a new one.
    /// </summary>
    /// <param name="dto">Inventory creation details</param>
    /// <returns>Existing or newly created inventory</returns>
    [HttpPost("check-or-create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckOrCreateInventory([FromBody] CreateInventoryDto dto)
    {
        try
        {
            // Validate input
            if (dto == null || dto.ProductId == Guid.Empty || dto.LocationId == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Product ID and Location ID are required",
                    errors = new
                    {
                        productId = dto?.ProductId == Guid.Empty ? "Product ID is required" : null,
                        locationId = dto?.LocationId == Guid.Empty ? "Location ID is required" : null
                    }
                });
            }

            if (string.IsNullOrWhiteSpace(dto.LocationType))
                dto.LocationType = "WAREHOUSE";

            _logger.LogInformation("Checking or creating inventory for product {ProductId} at {LocationType}:{LocationId}", 
                dto.ProductId, dto.LocationType, dto.LocationId);

            var inventory = await _inventoryService.CheckOrCreateInventoryAsync(dto);

            return Ok(new
            {
                success = true,
                message = "Inventory checked/created successfully",
                data = inventory
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking or creating inventory");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while checking or creating inventory",
                error = ex.Message
            });
        }
    }
}
