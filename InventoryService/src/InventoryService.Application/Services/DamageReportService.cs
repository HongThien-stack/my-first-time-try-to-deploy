using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class DamageReportService : IDamageReportService
{
    private readonly IDamageReportRepository _damageReportRepository;
    private readonly ILogger<DamageReportService> _logger;

    public DamageReportService(
        IDamageReportRepository damageReportRepository,
        ILogger<DamageReportService> logger)
    {
        _damageReportRepository = damageReportRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<DamageReportListDto>> GetAllDamageReportsAsync()
    {
        try
        {
            var reports = await _damageReportRepository.GetAllAsync();
            return reports.Select(r => new DamageReportListDto
            {
                Id = r.Id,
                ReportNumber = r.ReportNumber,
                LocationType = r.LocationType,
                LocationId = r.LocationId,
                DamageType = r.DamageType,
                ReportedBy = r.ReportedBy,
                ReportedDate = r.ReportedDate,
                TotalValue = r.TotalValue,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all damage reports");
            throw;
        }
    }

    public async Task<DamageReportDto?> GetDamageReportByIdAsync(Guid id)
    {
        try
        {
            var report = await _damageReportRepository.GetByIdAsync(id);
            if (report == null)
                return null;

            return new DamageReportDto
            {
                Id = report.Id,
                ReportNumber = report.ReportNumber,
                LocationType = report.LocationType,
                LocationId = report.LocationId,
                DamageType = report.DamageType,
                ReportedBy = report.ReportedBy,
                ReportedDate = report.ReportedDate,
                TotalValue = report.TotalValue,
                Description = report.Description,
                Photos = report.Photos,
                Status = report.Status,
                ApprovedBy = report.ApprovedBy,
                ApprovedDate = report.ApprovedDate,
                CreatedAt = report.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting damage report by id: {Id}", id);
            throw;
        }
    }
}
