using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IWarehouseSlotService
{
    Task<WarehouseSlotDto?> GetSlotByIdAsync(Guid id);
    Task<WarehouseSlotDto> CreateSlotAsync(Guid warehouseId, CreateSlotRequestDto request);
    Task<WarehouseSlotDto?> UpdateSlotAsync(Guid id, UpdateSlotRequestDto request);
    Task<bool> DeleteSlotAsync(Guid id);
}
