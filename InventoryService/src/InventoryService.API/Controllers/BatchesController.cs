using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/batches")]
[Authorize]
public class BatchesController : ControllerBase
{
    private readonly IProductBatchService _productBatchService;
    private readonly IBatchQueryService _batchQueryService;
    private readonly ILogger<BatchesController> _logger;

    public BatchesController(
        IProductBatchService productBatchService,
        IBatchQueryService batchQueryService,
        ILogger<BatchesController> logger)
    {
        _productBatchService = productBatchService;
        _batchQueryService = batchQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all product batches
    /// </summary>
    /// <returns>List of all batches</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var batches = await _productBatchService.GetAllAsync();
            return Ok(new
            {
                success = true,
                message = "Batches retrieved successfully",
                data = batches
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all batches");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving batches",
                error = ex.Message
            });
        }
    }

    [HttpGet("expiring-soon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringSoon()
    {
        try
        {
            var batches = await _batchQueryService.GetExpiringSoonBatchesAsync();
            return Ok(batches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring soon batches");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while retrieving expiring soon batches"
            });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var batch = await _batchQueryService.GetBatchByIdAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            return Ok(batch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch detail by ID: {BatchId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while retrieving batch detail"
            });
        }
    }
}
