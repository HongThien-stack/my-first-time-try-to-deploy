using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IDamageReportService
{
    Task<IEnumerable<DamageReportListDto>> GetAllDamageReportsAsync();
    Task<DamageReportDto?> GetDamageReportByIdAsync(Guid id);
    Task<DamageReportDto> CreateDamageReportAsync(CreateDamageReportRequest request);
    Task<DamageReportDto> ApproveDamageReportAsync(Guid id, ApproveDamageReportRequest request);
}
