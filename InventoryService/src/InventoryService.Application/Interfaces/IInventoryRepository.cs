using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IInventoryRepository
{
    Task<IEnumerable<Inventory>> GetAllAsync();
    Task<Inventory?> GetByIdAsync(Guid id);
    Task<IEnumerable<Inventory>> GetByLocationAsync(string locationType, Guid locationId);
    Task<IEnumerable<Inventory>> GetByProductIdAsync(Guid productId);
    Task<Inventory?> GetByLocationAndProductAsync(string locationType, Guid locationId, Guid productId);
    Task<IEnumerable<Inventory>> GetLowStockItemsAsync(string? locationType = null);
    Task<(IEnumerable<Inventory> Items, int TotalCount)> GetLowStockAlertsAsync(
        string? locationType = null,
        Guid? locationId = null,
        int pageNumber = 1,
        int pageSize = 20);
    Task<Inventory> AddAsync(Inventory inventory);
    Task UpdateAsync(Inventory inventory);
    Task UpdateReservedQuantityAsync(Inventory inventory);
    Task DeleteAsync(Guid id);
}
