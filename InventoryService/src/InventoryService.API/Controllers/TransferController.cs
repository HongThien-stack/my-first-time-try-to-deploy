using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;
        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpGet("transfers")]
        public async Task<ActionResult<List<TransferDto>>> GetAllTransfers()
        {
            var transfers = await _transferService.GetAllTransfersAsync();
            if (transfers == null || !transfers.Any())
            {
                return NotFound("No transfers found.");
            }
            List<TransferDto> combineTransfer = new List<TransferDto>();
            foreach (var transfer in transfers)
            {
                var transferItems = await _transferService.GetAllTransferItemsByIdAsync(transfer.Id);
                List<TransferItemDto> items = new List<TransferItemDto>();
                foreach (var item in transferItems)
                {
                    items.Add(new TransferItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        BatchId = item.BatchId,
                        RequestedQuantity = item.RequestedQuantity,
                        ShippedQuantity = item.ShippedQuantity,
                        ReceivedQuantity = item.ReceivedQuantity,
                        DamagedQuantity = item.DamagedQuantity,
                        Notes = item.Notes
                    });
                }
                combineTransfer.Add(new TransferDto
                {
                    Id = transfer.Id,
                    TransferNumber = transfer.TransferNumber,
                    FromLocationType = transfer.FromLocationType,
                    FromLocationId = transfer.FromLocationId,
                    ToLocationType = transfer.ToLocationType,
                    ToLocationId = transfer.ToLocationId,
                    TransferDate = transfer.TransferDate,
                    ExpectedDelivery = transfer.ExpectedDelivery,
                    ActualDelivery = transfer.ActualDelivery,
                    Status = transfer.Status,
                    ShippedBy = transfer.ShippedBy,
                    ReceivedBy = transfer.ReceivedBy,
                    Notes = transfer.Notes,
                    Items = items
                });
            }
            return Ok(combineTransfer);
        }

        [HttpGet("transfer/{id}")]
        public async Task<ActionResult<TransferDto>> GetTransferById([FromRoute] Guid id)
        {
            var transfer = await _transferService.GetTransferByIdAsync(id);
            if (transfer == null)
            {
                return NotFound("No transfer is found with this id");
            }
            var transferItems = await _transferService.GetAllTransferItemsByIdAsync(transfer.Id);
            List<TransferItemDto> items = new List<TransferItemDto>();
            foreach (var item in transferItems)
            {
                items.Add(new TransferItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    BatchId = item.BatchId,
                    RequestedQuantity = item.RequestedQuantity,
                    ShippedQuantity = item.ShippedQuantity,
                    ReceivedQuantity = item.ReceivedQuantity,
                    DamagedQuantity = item.DamagedQuantity,
                    Notes = item.Notes
                });
            }
            TransferDto transferDto = new TransferDto
            {
                Id = transfer.Id,
                TransferNumber = transfer.TransferNumber,
                FromLocationType = transfer.FromLocationType,
                FromLocationId = transfer.FromLocationId,
                ToLocationType = transfer.ToLocationType,
                ToLocationId = transfer.ToLocationId,
                TransferDate = transfer.TransferDate,
                ExpectedDelivery = transfer.ExpectedDelivery,
                ActualDelivery = transfer.ActualDelivery,
                Status = transfer.Status,
                ShippedBy = transfer.ShippedBy,
                ReceivedBy = transfer.ReceivedBy,
                Notes = transfer.Notes,
                Items = items
            };
            return Ok(transferDto);
        }

        [HttpPost("transfer")]
        public async Task<ActionResult> CreateTransfer([FromBody] CreateTransferDto createTransferDto)
        {
            Guid transferId = Guid.NewGuid();
            var count = await _transferService.CountTransferAsync();
            int orderCount = count + 1;
            await _transferService.AddNewTransferAsync(new Transfer{
                Id = transferId,
                TransferNumber = $"TRF-{DateTime.UtcNow.Year}-{orderCount:D3}",
                FromLocationType = createTransferDto.FromLocationType,
                FromLocationId = createTransferDto.FromLocationId,
                ToLocationType = createTransferDto.ToLocationType,
                ToLocationId = createTransferDto.ToLocationId,
                TransferDate = DateTime.Now,
                ExpectedDelivery = createTransferDto.ExpectedDelivery,
                ActualDelivery = null,
                Status = "PENDING",
                ShippedBy = createTransferDto.ShippedBy,
                ReceivedBy = null,
                Notes = createTransferDto.Notes,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            });
            foreach (var item in createTransferDto.Items)
            {
                await _transferService.AddNewTransferItemAsync(new TransferItem
                {
                    Id = Guid.NewGuid(),
                    TransferId = transferId,
                    ProductId = item.ProductId,
                    BatchId = item.BatchId,
                    RequestedQuantity = item.RequestedQuantity,
                    ShippedQuantity = item.RequestedQuantity,
                    ReceivedQuantity = item.ReceivedQuantity,
                    DamagedQuantity = item.RequestedQuantity - item.ReceivedQuantity,
                    Notes = item.Notes
                });
            }
            return Ok("Transfer created successfully.");
        }
    }
}
