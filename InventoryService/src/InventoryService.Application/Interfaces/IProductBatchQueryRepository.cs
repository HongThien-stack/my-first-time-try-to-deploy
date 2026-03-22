using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchQueryRepository
{
    Task<BatchDetailDto?> GetBatchDetailByIdAsync(Guid id);
    Task<IEnumerable<ExpiringSoonBatchDto>> GetExpiringSoonBatchesAsync();
    Task<IEnumerable<BatchDetailDto>> GetBatchesByWarehouseIdAsync(Guid warehouseId);
}
