using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IStockMovementRepository
{
    Task<IEnumerable<StockMovement>> GetAllAsync();
    Task<StockMovement?> GetByIdAsync(Guid id);
    Task<StockMovement?> GetByMovementNumberAsync(string movementNumber);
    Task<IEnumerable<StockMovement>> GetByLocationAsync(string locationType, Guid locationId);
    Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(string movementType);
    Task<int> CountByDateAsync(DateTime date);
    Task<StockMovement> AddAsync(StockMovement movement);
    Task AddNewStockMovementAsync(StockMovement stockMovement);
    Task AddNewStockMovementItemAsync(StockMovementItem stockMovementItem);
    Task<int> CountStockMovementAsync();
    Task UpdateAsync(StockMovement movement);
    Task DeleteAsync(Guid id);
}
