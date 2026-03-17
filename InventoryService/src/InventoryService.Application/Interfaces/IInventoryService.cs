using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IInventoryService
{
    Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync();
    Task<InventoryDto?> GetInventoryByIdAsync(Guid id);
    Task<IEnumerable<InventoryDto>> GetInventoriesByLocationAsync(string locationType, Guid locationId);
    Task<IEnumerable<InventoryDto>> GetInventoriesByProductAsync(Guid productId);
    Task<IEnumerable<InventoryDto>> GetLowStockItemsAsync(string? locationType = null);
    Task<Inventory?> GetInventoryByLocationIdAndProductIdAsync(Guid deliverWarehouseId, Guid productId);
    Task<InventoryDto> UpdateInventoryAsync(Guid id, int quantity, Guid performedBy, string reason);
    Task UpdateReservedQuantityAsync(Inventory inventory);
    Task<InventoryDto> CheckOrCreateInventoryAsync(CreateInventoryDto dto);
}
