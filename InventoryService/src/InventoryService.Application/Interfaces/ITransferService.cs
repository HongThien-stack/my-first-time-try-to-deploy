using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface ITransferService
{
    Task<List<Transfer>> GetAllTransfersAsync();
    Task<List<TransferItem>> GetAllTransferItemsByIdAsync(Guid transferId);
    Task<Transfer?> GetTransferByIdAsync(Guid id);
    Task AddNewTransferAsync(Transfer transfer);
    Task AddNewTransferItemAsync(TransferItem transferItem);
    Task<int> CountTransferAsync();
}
