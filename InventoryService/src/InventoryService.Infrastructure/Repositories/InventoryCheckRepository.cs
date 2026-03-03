using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class InventoryCheckRepository : IInventoryCheckRepository
{
    private readonly InventoryDbContext _context;

    public InventoryCheckRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryCheck>> GetAllAsync()
    {
        return await _context.InventoryChecks
            .Include(c => c.InventoryCheckItems)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<InventoryCheck?> GetByIdAsync(Guid id)
    {
        return await _context.InventoryChecks
            .Include(c => c.InventoryCheckItems)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<InventoryCheck?> GetByCheckNumberAsync(string checkNumber)
    {
        return await _context.InventoryChecks
            .Include(c => c.InventoryCheckItems)
            .FirstOrDefaultAsync(c => c.CheckNumber == checkNumber);
    }

    public async Task<IEnumerable<InventoryCheck>> GetByLocationAsync(string locationType, Guid locationId)
    {
        return await _context.InventoryChecks
            .Include(c => c.InventoryCheckItems)
            .Where(c => c.LocationType == locationType && c.LocationId == locationId)
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryCheck>> GetByStatusAsync(string status)
    {
        return await _context.InventoryChecks
            .Include(c => c.InventoryCheckItems)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<InventoryCheck> AddAsync(InventoryCheck check)
    {
        check.CreatedAt = DateTime.UtcNow;
        _context.InventoryChecks.Add(check);
        await _context.SaveChangesAsync();
        return check;
    }

    public async Task UpdateAsync(InventoryCheck check)
    {
        _context.InventoryChecks.Update(check);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var check = await _context.InventoryChecks.FindAsync(id);
        if (check != null)
        {
            _context.InventoryChecks.Remove(check);
            await _context.SaveChangesAsync();
        }
    }
}
