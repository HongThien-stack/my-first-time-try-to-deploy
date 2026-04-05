using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductBatchController : ControllerBase
{
    private readonly IProductBatchService _productBatchService;
    private readonly ILogger<ProductBatchController> _logger;

    public ProductBatchController(IProductBatchService productBatchService, ILogger<ProductBatchController> logger)
    {
        _productBatchService = productBatchService;
        _logger = logger;
    }

    [HttpGet("batches")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductBatchDto>>> GetAllBatches()
    {
        _logger.LogInformation("Getting all product batches");
        var batches = await _productBatchService.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ProductBatchDto>>> GetBatchesByWarehouse([FromRoute] Guid warehouseId)
    {
        _logger.LogInformation("Getting product batches for warehouse {WarehouseId}", warehouseId);
        var batches = await _productBatchService.GetByWarehouseIdAsync(warehouseId);
        if (!batches.Any())
            return NotFound(new { success = false, message = "No batches found for this warehouse" });
        
        return Ok(new
        {
            success = true,
            data = batches,
            count = batches.Count()
        });
    }

    [HttpGet("batch/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductBatchDto>> GetBatchById([FromRoute] Guid id)
    {
        _logger.LogInformation("Getting batch {BatchId}", id);
        var batch = await _productBatchService.GetByIdAsync(id);
        if (batch == null)
            return NotFound(new { success = false, message = "Batch not found" });
        return Ok(batch);
    }

    /// <summary>
    /// Allocate/split a product batch: create a new batch from an existing batch
    /// with the specified quantity. The source batch quantity is reduced by the allocated amount.
    /// </summary>
    [HttpPost("batch/allocate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductBatchDto>> AllocateBatch([FromBody] CreateAllocatedBatchDto dto)
    {
        try
        {
            _logger.LogInformation("Allocating batch: source {SourceBatchId}, qty {Qty}", dto.SourceBatchId, dto.AllocatedQuantity);

            if (dto.AllocatedQuantity <= 0)
                return BadRequest(new { success = false, message = "Allocated quantity must be greater than 0" });

            var result = await _productBatchService.AllocateBatchAsync(dto);
            return Ok(new
            {
                success = true,
                message = "Batch allocated successfully",
                data = result
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Allocate batch failed: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating batch");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while allocating batch",
                error = ex.Message
            });
        }
    }

    [HttpPost("receive-from-supplier")]
    [ProducesResponseType(typeof(IEnumerable<ProductBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReceiveFromSupplier([FromBody] ReceiveFromSupplierDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var receivedById))
            {
                return Unauthorized(new { success = false, message = "Invalid or missing user identity in token" });
            }
            dto.ReceivedBy = receivedById;
            var result = await _productBatchService.ReceiveFromSupplierAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while receiving goods from supplier for request {RestockRequestId}", dto.RestockRequestId);
            return StatusCode(500, new { success = false, message = "An internal error occurred." });
        }
    }

    /// <summary>
    /// Create outbound stock movement for all expired batches at a location
    /// Only Admin, Manager, Warehouse Manager, and Warehouse Admin can process expired batches
    /// </summary>
    [HttpPost("expired-batches/create-outbound")]
    [Authorize(Roles = "Admin,Manager,Warehouse Manager,Warehouse Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOutboundFromExpiredBatches([FromBody] CreateOutboundFromExpiredBatchesDto request)
    {
        try
        {
            var locationType = string.IsNullOrWhiteSpace(request.LocationType)
                ? "WAREHOUSE"
                : request.LocationType.Trim().ToUpperInvariant();

            var locationId = request.LocationId ?? request.WarehouseId;

            _logger.LogInformation(
                "Creating outbound stock movement for expired batches at {LocationType} {LocationId}",
                locationType,
                locationId);

            // Validate input
            if (locationId == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "LocationId (or WarehouseId for backward compatibility) must be a valid non-empty GUID"
                });
            }

            if (locationType is not ("WAREHOUSE" or "STORE"))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "LocationType must be one of: WAREHOUSE, STORE"
                });
            }

            var result = await _productBatchService.CreateOutboundFromExpiredBatchesAsync(request);
            return Ok(new
            {
                success = true,
                message = "Outbound stock movement created for expired batches",
                data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot create outbound movement: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            var locationId = request.LocationId ?? request.WarehouseId;
            _logger.LogError(ex, "Error creating outbound for expired batches at location {LocationId}", locationId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating outbound stock movement",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Adjust batch quantity by physical count and synchronize inventory for the same product/location.
    /// </summary>
    [HttpPost("batch/adjust-quantity")]
    [Authorize(Roles = "Admin,Store Manager,Warehouse Manager,Warehouse Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AdjustBatchQuantity([FromBody] AdjustBatchInventoryDto request)
    {
        try
        {
            if (request.BatchId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "BatchId must be a valid non-empty GUID" });
            }

            if (request.ActualQuantity < 0)
            {
                return BadRequest(new { success = false, message = "ActualQuantity cannot be negative" });
            }

            Guid? adjustedBy = null;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                adjustedBy = parsedUserId;
            }

            var result = await _productBatchService.AdjustBatchAndInventoryAsync(request, adjustedBy);
            return Ok(new
            {
                success = true,
                message = "Batch and inventory adjusted successfully",
                data = result
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Adjust batch quantity failed: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting batch quantity for BatchId {BatchId}", request.BatchId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while adjusting batch and inventory",
                error = ex.Message
            });
        }
    }
}
