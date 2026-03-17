using System.Security.Claims;
using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/restock-requests")]
[Authorize(Roles = "Admin,Store Manager,Warehouse Manager,Warehouse Admin")]
public class RestockRequestsController : ControllerBase
{
    private readonly IRestockRequestService _restockRequestService;
    private readonly ILogger<RestockRequestsController> _logger;

    public RestockRequestsController(
        IRestockRequestService restockRequestService,
        ILogger<RestockRequestsController> logger)
    {
        _restockRequestService = restockRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Get all restock requests
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var requests = await _restockRequestService.GetAllRequestsAsync();
            return Ok(new
            {
                success = true,
                message = "Restock requests retrieved successfully",
                data = requests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all restock requests");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving restock requests",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get all restock requests where the given warehouse is either the source or destination.
    /// </summary>
    [HttpGet("by-warehouse/{warehouseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
    {
        try
        {
            var requests = await _restockRequestService.GetRequestsByWarehouseAsync(warehouseId);
            return Ok(new
            {
                success = true,
                message = "Restock requests retrieved successfully",
                data = requests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restock requests for warehouse {WarehouseId}", warehouseId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving restock requests",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get all restock requests whose from/to warehouse is a child of the given parent warehouse.
    /// Used by Warehouse Admin to see all requests under their Kho Tổng.
    /// </summary>
    [HttpGet("by-parent-warehouse/{parentWarehouseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByParentWarehouse(Guid parentWarehouseId)
    {
        try
        {
            var requests = await _restockRequestService.GetRequestsByParentWarehouseAsync(parentWarehouseId);
            return Ok(new
            {
                success = true,
                message = "Restock requests retrieved successfully",
                data = requests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restock requests for parent warehouse {ParentWarehouseId}", parentWarehouseId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving restock requests",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get restock request by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var request = await _restockRequestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Restock request not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Restock request retrieved successfully",
                data = request
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restock request {RequestId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving restock request",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new restock request from store
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateRestockRequestDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var requestedBy))
                return Unauthorized(new { success = false, message = "Invalid or missing user identity in token" });

            var created = await _restockRequestService.CreateRequestAsync(dto, requestedBy);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
            {
                success = true,
                message = "Restock request created successfully",
                data = created
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Create restock request failed: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating restock request");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating restock request",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Approve a restock request (sets status to APPROVED).
    /// </summary>
    [HttpPut("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var approvedBy))
                return Unauthorized(new { success = false, message = "Invalid or missing user identity in token" });

            var result = await _restockRequestService.ApproveRequestAsync(id, approvedBy);
            return Ok(new
            {
                success = true,
                message = "Restock request approved successfully",
                data = result
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Approve restock request failed: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving restock request {RequestId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while approving restock request",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Reject a restock request.
    /// </summary>
    [HttpPut("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRestockRequestDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var rejectedBy))
                return Unauthorized(new { success = false, message = "Invalid or missing user identity in token" });

            await _restockRequestService.RejectRequestAsync(id, rejectedBy, dto);
            return Ok(new
            {
                success = true,
                message = "Restock request rejected successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Reject restock request failed: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting restock request {RequestId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while rejecting restock request",
                error = ex.Message
            });
        }
    }
}
