using Azure.Core;
using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Services;
using InventoryService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace InventoryService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly IInventoryService _inventoryService;
        private readonly IStockMovementService _stockMovementService;
        private readonly IRestockRequestService _restockRequestService;
        private readonly ILogger<TransferController> _logger;

        public TransferController(ITransferService transferService, ILogger<TransferController> logger,
            IInventoryService inventoryService, IStockMovementService stockMovementService, IRestockRequestService restockRequestService)
        {
            _transferService = transferService;
            _logger = logger;
            _inventoryService = inventoryService;
            _stockMovementService = stockMovementService;
            _restockRequestService = restockRequestService;
        }

        [HttpGet("transfers")]
        public async Task<ActionResult<IEnumerable<TransferDto>>> GetAllTransfers()
        {
            var transfers = await _transferService.GetAllTransfersAsync();
            return Ok(transfers);
        }

        [HttpGet("transfer/{id}")]
        public async Task<ActionResult<TransferDto>> GetTransferById([FromRoute] Guid id)
        {
            var transfer = await _transferService.GetTransferByIdAsync(id);
            if (transfer == null)
                return NotFound("No transfer is found with this id");
            return Ok(transfer);
        }

        [HttpPost("transfer")]
        public async Task<ActionResult<TransferDto>> CreateTransfer([FromBody] CreateTransferDto createTransferDto)
        {
            var result = await _transferService.CreateTransferAsync(createTransferDto);
            return Ok(result);
        }

        [HttpPut("transfer/{id}/status")]
        public async Task<ActionResult> UpdateTransferStatus(
            [FromRoute] Guid id,
            [FromBody] UpdateTransferStatusDto dto)
        {
            var transfer = await _transferService.GetTransferByIdAsync(id);
            if (transfer == null)
                return NotFound("No transfer is found with this id.");

            await _transferService.UpdateTransferStatusAsync(id, dto.Status);
            return Ok("Transfer status updated successfully.");
        }

        [HttpDelete("transfer/{id}")]
        public async Task<ActionResult> DeleteTransfer([FromRoute] Guid id)
        {
            await _transferService.DeleteTransferAsync(id);
            return Ok("Transfer deleted successfully.");
        }

        /// <summary>
        /// Nhận hàng (nhập kho): cập nhật số lượng thực nhận, hàng hư, tạo StockMovement,
        /// tự động set Transfer và RestockRequest thành COMPLETED.
        /// </summary>
        [HttpPut("transfer/{id}/receive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReceiveTransfer([FromRoute] Guid id, [FromBody] ReceiveTransferDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
                if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var receivedBy))
                    return Unauthorized(new { success = false, message = "Invalid or missing user identity in token" });

                var result = await _transferService.ReceiveTransferAsync(id, dto, receivedBy);
                return Ok(new
                {
                    success = true,
                    message = "Transfer received and marked as COMPLETED",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Receive transfer failed: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving transfer {TransferId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while receiving transfer",
                    error = ex.Message
                });
            }
        }

        //Luồng xuất kho
        [HttpPatch("transferV2/{transferId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateStockMovement([FromRoute] Guid transferId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
                if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var shippedBy))
                    return Unauthorized(new { success = false, message = "Invalid or missing user identity in token" });

                var result = await _transferService.CreateOutboundStockMovementAsync(transferId, shippedBy);
                //Cập nhật trạng thái Restock Request
                var transfer = await _transferService.GetByTransferIdWithoutTransferItemAsync(transferId);
                if (transfer != null && transfer.RestockRequestId.HasValue)
                {
                    var restockRequest = await _restockRequestService.GetByRequestIdWithoutRestockRequestItemAsync(transfer.RestockRequestId.Value);
                    if (restockRequest != null)
                    {
                        restockRequest.Status = "PROCESSING";
                        restockRequest.UpdatedAt = DateTime.UtcNow;
                        await _restockRequestService.UpdateAsync(restockRequest);
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Outbound stock movement created successfully",
                    data = result
                });
                
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating outbound stock movement for transfer {TransferId}", transferId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating stock movement",
                    error = ex.Message
                });
            }
        }
    }
}
