using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/batches")]
public class BatchesController : ControllerBase
{
    private readonly IBatchQueryService _batchQueryService;
    private readonly ILogger<BatchesController> _logger;

    public BatchesController(
        IBatchQueryService batchQueryService,
        ILogger<BatchesController> logger)
    {
        _batchQueryService = batchQueryService;
        _logger = logger;
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
