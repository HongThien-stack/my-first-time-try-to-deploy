using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
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
    }
}
