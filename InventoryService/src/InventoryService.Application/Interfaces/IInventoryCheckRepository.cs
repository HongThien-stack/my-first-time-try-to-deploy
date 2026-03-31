using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IInventoryCheckRepository
{
    Task<IEnumerable<InventoryCheck>> GetAllAsync(int? year = null, int? month = null);
    Task<InventoryCheck?> GetByIdAsync(Guid id);
    Task<InventoryCheck?> GetByCheckNumberAsync(string checkNumber);
    Task<IEnumerable<InventoryCheck>> GetByLocationAsync(string locationType, Guid locationId);
    Task<IEnumerable<InventoryCheck>> GetByStatusAsync(string status);
    Task<InventoryCheck> AddAsync(InventoryCheck check);
    Task UpdateAsync(InventoryCheck check);
    Task DeleteAsync(Guid id);
}
