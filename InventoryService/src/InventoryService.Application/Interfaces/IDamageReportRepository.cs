using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IDamageReportRepository
{
    Task<IEnumerable<DamageReport>> GetAllAsync();
    Task<DamageReport?> GetByIdAsync(Guid id);
    Task<DamageReport?> GetByReportNumberAsync(string reportNumber);
    Task<IEnumerable<DamageReport>> GetByLocationAsync(string locationType, Guid locationId);
    Task<IEnumerable<DamageReport>> GetByStatusAsync(string status);
    Task<IEnumerable<DamageReport>> GetByDamageTypeAsync(string damageType);
    Task<DamageReport> AddAsync(DamageReport report);
    Task UpdateAsync(DamageReport report);
    Task DeleteAsync(Guid id);
}
