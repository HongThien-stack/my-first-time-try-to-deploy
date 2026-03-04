using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IDamageReportService
{
    Task<IEnumerable<DamageReportListDto>> GetAllDamageReportsAsync();
    Task<DamageReportDto?> GetDamageReportByIdAsync(Guid id);
}
