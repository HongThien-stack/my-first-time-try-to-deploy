using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IInventoryRepository
{
    Task<IEnumerable<Inventory>> GetAllAsync();
    Task<Inventory?> GetByIdAsync(Guid id);
    Task<IEnumerable<Inventory>> GetByStoreIdAsync(Guid storeId);
    Task<IEnumerable<Inventory>> GetByProductIdAsync(Guid productId);
    Task<Inventory?> GetByStoreAndProductAsync(Guid storeId, Guid productId);
    Task<Inventory> AddAsync(Inventory inventory);
    Task UpdateAsync(Inventory inventory);
    Task DeleteAsync(Guid id);
}
