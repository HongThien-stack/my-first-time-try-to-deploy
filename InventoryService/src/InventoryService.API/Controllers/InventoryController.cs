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

    /// <summary>
    /// Update minimum stock level for a specific inventory record.
    /// </summary>
    /// <param name="inventoryId">Inventory ID</param>
    /// <param name="dto">New minimum stock level payload</param>
    /// <returns>Updated inventory</returns>
    [HttpPut("{inventoryId:guid}/min-stock-level")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMinStockLevel(Guid inventoryId, [FromBody] UpdateMinStockLevelDto dto)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request body is required"
                });
            }

            var updatedInventory = await _inventoryService.UpdateMinStockLevelAsync(inventoryId, dto.MinStockLevel);

            return Ok(new
            {
                success = true,
                message = "Minimum stock level updated successfully",
                data = updatedInventory
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating min stock level for inventory {InventoryId}", inventoryId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while updating min stock level",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get low stock alerts - inventory items where available quantity is at or below minimum stock level
    /// </summary>
    /// <param name="locationType">Optional filter: Location type (WAREHOUSE or STORE)</param>
    /// <param name="locationId">Optional filter: Specific location ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paginated list of low stock items with product details and stock status</returns>
    [HttpGet("low-stock-alerts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLowStockAlerts(
        [FromQuery] string? locationType = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page number must be greater than 0"
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page size must be between 1 and 100"
                });
            }

            // Validate location type if provided
            if (!string.IsNullOrEmpty(locationType) && 
                locationType != "WAREHOUSE" && 
                locationType != "STORE")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Location type must be either 'WAREHOUSE' or 'STORE'"
                });
            }

            _logger.LogInformation(
                "Fetching low stock alerts - LocationType: {LocationType}, LocationId: {LocationId}, Page: {PageNumber}, PageSize: {PageSize}",
                locationType, locationId, pageNumber, pageSize);

            var result = await _inventoryService.GetLowStockAlertsAsync(
                locationType, locationId, pageNumber, pageSize);

            return Ok(new
            {
                success = true,
                message = $"Low stock alerts retrieved successfully ({result.TotalItems} items found)",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock alerts");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving low stock alerts",
                error = ex.Message
            });
        }
    }
}
