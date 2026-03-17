using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IStockMovementService
{
    Task<IEnumerable<StockMovementDto>> GetAllAsync();
    Task<StockMovementDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockMovementItemDto>> GetItemsByMovementIdAsync(Guid movementId);
    Task AddNewStockMovementAsync(StockMovement stockMovement);
    Task AddNewStockMovementItemAsync(StockMovementItem stockMovementItem);
    Task<int> CountStockMovementAsync();
}