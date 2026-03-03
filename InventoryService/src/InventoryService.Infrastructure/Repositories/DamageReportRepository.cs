using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class DamageReportRepository : IDamageReportRepository
{
    private readonly InventoryDbContext _context;

    public DamageReportRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DamageReport>> GetAllAsync()
    {
        return await _context.DamageReports
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<DamageReport?> GetByIdAsync(Guid id)
    {
        return await _context.DamageReports
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DamageReport?> GetByReportNumberAsync(string reportNumber)
    {
        return await _context.DamageReports
            .FirstOrDefaultAsync(d => d.ReportNumber == reportNumber);
    }

    public async Task<IEnumerable<DamageReport>> GetByLocationAsync(string locationType, Guid locationId)
    {
        return await _context.DamageReports
            .Where(d => d.LocationType == locationType && d.LocationId == locationId)
            .OrderByDescending(d => d.ReportedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DamageReport>> GetByStatusAsync(string status)
    {
        return await _context.DamageReports
            .Where(d => d.Status == status)
            .OrderByDescending(d => d.ReportedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DamageReport>> GetByDamageTypeAsync(string damageType)
    {
        return await _context.DamageReports
            .Where(d => d.DamageType == damageType)
            .OrderByDescending(d => d.ReportedDate)
            .ToListAsync();
    }

    public async Task<DamageReport> AddAsync(DamageReport report)
    {
        report.CreatedAt = DateTime.UtcNow;
        _context.DamageReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task UpdateAsync(DamageReport report)
    {
        _context.DamageReports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var report = await _context.DamageReports.FindAsync(id);
        if (report != null)
        {
            _context.DamageReports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
}
