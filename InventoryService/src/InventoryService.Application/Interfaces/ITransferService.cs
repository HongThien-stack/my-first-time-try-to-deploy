using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface ITransferService
{
    Task<IEnumerable<TransferDto>> GetAllTransfersAsync();
    Task<TransferDto?> GetTransferByIdAsync(Guid id);
    Task<TransferDto?> GetTransferByNumberAsync(string transferNumber);
    Task<IEnumerable<TransferDto>> GetTransfersByStatusAsync(string status);
    Task<TransferDto> CreateTransferAsync(CreateTransferDto dto);
    Task UpdateTransferStatusAsync(Guid id, string status, Guid? userId = null);
    Task<bool> DeleteTransferAsync(Guid id);
    Task<TransferDto> ReceiveTransferAsync(Guid transferId, ReceiveTransferDto dto, Guid receivedBy);
    Task<bool> CreateOutboundStockMovementAsync(Guid transferId, Guid shippedBy);
}
