using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IStockMovementService
{
    Task<IEnumerable<StockMovementDto>> GetAllAsync();
    Task<StockMovementDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockMovementItemDto>> GetItemsByMovementIdAsync(Guid movementId);
}