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
    /// Get inventories by store ID
    /// </summary>
    /// <param name="storeId">Store ID</param>
    /// <returns>List of inventories for the store</returns>
    [HttpGet("store/{storeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        try
        {
            var inventories = await _inventoryService.GetInventoriesByStoreAsync(storeId);
            return Ok(new
            {
                success = true,
                message = $"Inventories for store {storeId} retrieved successfully",
                data = inventories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventories for store {StoreId}", storeId);
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
}
