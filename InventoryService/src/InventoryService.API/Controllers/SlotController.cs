using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/slots")]
public class SlotController : ControllerBase
{
    private readonly IWarehouseSlotService _slotService;
    private readonly ILogger<SlotController> _logger;

    public SlotController(IWarehouseSlotService slotService, ILogger<SlotController> logger)
    {
        _slotService = slotService;
        _logger = logger;
    }

    /// <summary>
    /// Get slot details by ID
    /// </summary>
    /// <param name="id">Slot ID</param>
    /// <returns>Slot details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var slot = await _slotService.GetSlotByIdAsync(id);
            if (slot == null)
                return NotFound(new { success = false, message = "Slot not found" });

            return Ok(new
            {
                success = true,
                message = "Slot retrieved successfully",
                data = slot
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slot {SlotId}", id);
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving slot" });
        }
    }

    /// <summary>
    /// Update slot information
    /// </summary>
    /// <param name="id">Slot ID</param>
    /// <param name="request">Updated slot data</param>
    /// <returns>Updated slot</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSlotRequestDto request)
    {
        try
        {
            var updated = await _slotService.UpdateSlotAsync(id, request);
            if (updated == null)
                return NotFound(new { success = false, message = "Slot not found" });

            return Ok(new
            {
                success = true,
                message = "Slot updated successfully",
                data = updated
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Update slot {SlotId} failed: {Message}", id, ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating slot {SlotId}", id);
            return StatusCode(500, new { success = false, message = "An error occurred while updating slot" });
        }
    }

    /// <summary>
    /// Delete a slot (only allowed when status is not OCCUPIED)
    /// </summary>
    /// <param name="id">Slot ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _slotService.DeleteSlotAsync(id);
            if (!deleted)
                return NotFound(new { success = false, message = "Slot not found" });

            return Ok(new { success = true, message = "Slot deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Delete slot {SlotId} failed: {Message}", id, ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting slot {SlotId}", id);
            return StatusCode(500, new { success = false, message = "An error occurred while deleting slot" });
        }
    }
}
