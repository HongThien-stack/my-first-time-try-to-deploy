using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchQueryRepository
{
    Task<BatchDetailDto?> GetBatchDetailByIdAsync(Guid id);
    Task<IEnumerable<ExpiringSoonBatchDto>> GetExpiringSoonBatchesAsync();
}
