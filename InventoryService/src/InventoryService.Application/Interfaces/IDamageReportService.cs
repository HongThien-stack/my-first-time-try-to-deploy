using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IDamageReportService
{
    Task<IEnumerable<DamageReportListDto>> GetAllDamageReportsAsync();
    Task<DamageReportDto?> GetDamageReportByIdAsync(Guid id);
    Task<DamageReportDto> CreateDamageReportAsync(CreateDamageReportRequest request, Guid currentUserId);
    Task<DamageReportDto> ApproveDamageReportAsync(Guid id, Guid currentUserId);
}
