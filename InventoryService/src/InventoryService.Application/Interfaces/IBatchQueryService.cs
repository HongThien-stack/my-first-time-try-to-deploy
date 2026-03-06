using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IBatchQueryService
{
    Task<BatchDetailDto?> GetBatchByIdAsync(Guid id);
    Task<IEnumerable<ExpiringSoonBatchDto>> GetExpiringSoonBatchesAsync();
}
