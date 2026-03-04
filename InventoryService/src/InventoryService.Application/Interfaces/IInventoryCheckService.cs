using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IInventoryCheckService
{
    Task<IEnumerable<InventoryCheckListDto>> GetAllInventoryChecksAsync();
    Task<InventoryCheckDto?> GetInventoryCheckByIdAsync(Guid id);
}
