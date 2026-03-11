using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/stock-movements")]
[Authorize]
public class StockMovementsController : ControllerBase
{
    private readonly IStockMovementService _stockMovementService;
    private readonly ILogger<StockMovementsController> _logger;

    public StockMovementsController(
        IStockMovementService stockMovementService,
        ILogger<StockMovementsController> logger)
    {
        _stockMovementService = stockMovementService;
        _logger = logger;
    }

    /// <summary>
    /// Get all stock movements history
    /// </summary>
    /// <returns>List of all stock movements</returns>
    [HttpGet("get-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var movements = await _stockMovementService.GetAllAsync();
            return Ok(new
            {
                success = true,
                message = "Stock movements retrieved successfully",
                data = movements
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all stock movements");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving stock movements",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get stock movement detail by ID
    /// </summary>
    /// <param name="id">Stock movement ID</param>
    /// <returns>Stock movement details</returns>
    [HttpGet("Get-detail-movement-by-id")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var movement = await _stockMovementService.GetByIdAsync(id);

            if (movement == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Stock movement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Stock movement retrieved successfully",
                data = movement
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock movement {MovementId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving stock movement",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get items of a stock movement
    /// </summary>
    /// <param name="id">Stock movement ID</param>
    /// <returns>List of items in the movement</returns>
    [HttpGet("get-items-by-movement-id")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItems(Guid id)
    {
        try
        {
            var items = await _stockMovementService.GetItemsByMovementIdAsync(id);
            return Ok(new
            {
                success = true,
                message = "Stock movement items retrieved successfully",
                data = items
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items for stock movement {MovementId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving stock movement items",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Lấy UserId từ JWT token claim
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}