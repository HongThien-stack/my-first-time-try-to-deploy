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
        private readonly ILogger<TransferController> _logger;

        public TransferController(ITransferService transferService, ILogger<TransferController> logger,
            IInventoryService inventoryService, IStockMovementService stockMovementService)
        {
            _transferService = transferService;
            _logger = logger;
            _inventoryService = inventoryService;
            _stockMovementService = stockMovementService;
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

        [HttpPatch("transferV2/{transferId}")]
        public async Task<ActionResult> CreateStockMovement([FromRoute] Guid transferId)
        {
            //1. Lấy transfer theo id
            var transfer = await _transferService.GetTransferByIdAsync(transferId);
            if (transfer == null)
                return NotFound("No transfer is found with this id.");
            var deliverWarehouseId = transfer.FromLocationId;
            //2. Lấy transfer items theo transferId
            var transferItems = transfer.Items;
            //3. Lấy inventory theo deliverWarehouseId và productId
            foreach (var item in transferItems)
            {
                var inventory = await _inventoryService.GetInventoryByLocationIdAndProductIdAsync(deliverWarehouseId, item.ProductId);
                if (inventory != null)
                {
                    inventory.Quantity -= item.RequestedQuantity;
                    inventory.ReservedQuantity -= item.RequestedQuantity;
                    await _inventoryService.UpdateReservedQuantityAsync(inventory);
                }
            }
            //4. Tạo StockMovement
            Guid movementId = Guid.NewGuid();
            var count = await _stockMovementService.CountStockMovementAsync();
            int order = count + 1;
            StockMovement stockMovement = new StockMovement
            {
                Id = movementId,
                MovementNumber = $"SM-{DateTime.UtcNow.Year}-{order:D3}",
                MovementType = "OUTBOUND",
                LocationId = transfer.FromLocationId,
                LocationType = "WAREHOUSE",
                MovementDate = transfer.TransferDate,
                RestockRequestId = transfer.RestockRequestId,
                SupplierName = null,
                TransferId = transfer.Id,
                ReceivedBy = transfer.ReceivedBy,
                Status = "COMPLETE",
                Notes = null,
                CreatedAt = DateTime.UtcNow
            };
            await _stockMovementService.AddNewStockMovementAsync(stockMovement);
            //5. Tạo StockMovementItem
            foreach (var item in transferItems)
            {
                var inventory = await _inventoryService.GetInventoryByLocationIdAndProductIdAsync(deliverWarehouseId, item.ProductId);
                if (inventory != null)
                {
                    StockMovementItem stockMovementItem = new StockMovementItem
                    {
                        Id = Guid.NewGuid(),
                        MovementId = movementId,
                        ProductId = item.ProductId,
                        Quantity = item.RequestedQuantity,
                        UnitPrice = 35000
                    };
                    await _stockMovementService.AddNewStockMovementItemAsync(stockMovementItem);
                }
            }
            return Ok("Stock movement created successfully.");
        }
    }
}
