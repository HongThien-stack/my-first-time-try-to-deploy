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
    private readonly ILogger<BatchesController> _logger;

    public BatchesController(
        IProductBatchService productBatchService,
        ILogger<BatchesController> logger)
    {
        _productBatchService = productBatchService;
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
}