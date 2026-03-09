using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IWarehouseSlotRepository
{
    Task<WarehouseSlot?> GetByIdAsync(Guid id);
    Task<IEnumerable<WarehouseSlot>> GetByWarehouseIdAsync(Guid warehouseId);
    Task<bool> ExistsSlotCodeAsync(Guid warehouseId, string slotCode, Guid? excludeId = null);
    Task<WarehouseSlot> AddAsync(WarehouseSlot slot);
    Task UpdateAsync(WarehouseSlot slot);
    Task DeleteAsync(Guid id);
}
