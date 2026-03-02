using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IInventoryService
{
    Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync();
    Task<InventoryDto?> GetInventoryByIdAsync(Guid id);
    Task<IEnumerable<InventoryDto>> GetInventoriesByStoreAsync(Guid storeId);
    Task<IEnumerable<InventoryDto>> GetInventoriesByProductAsync(Guid productId);
}
