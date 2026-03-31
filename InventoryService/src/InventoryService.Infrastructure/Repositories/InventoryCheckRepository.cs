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

    public async Task<IEnumerable<InventoryCheck>> GetAllAsync(int? year = null, int? month = null)
    {
        var query = _context.InventoryChecks
            .Include(c => c.InventoryCheckItems)
            .AsQueryable();

        if (year.HasValue && month.HasValue)
        {
            var startOfMonth = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfNextMonth = startOfMonth.AddMonths(1);
            query = query.Where(c => c.CheckDate >= startOfMonth && c.CheckDate < startOfNextMonth);
        }

        return await query
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
        // Detach any existing tracked entity with the same key
        var local = _context.Set<InventoryCheck>()
            .Local
            .FirstOrDefault(e => e.Id == check.Id);
        if (local != null)
        {
            _context.Entry(local).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }

        // Attach and mark specific properties as modified
        _context.Attach(check);
        _context.Entry(check).Property(c => c.Status).IsModified = true;
        _context.Entry(check).Property(c => c.TotalDiscrepancies).IsModified = true;
        _context.Entry(check).Property(c => c.Notes).IsModified = true;

        // Only add items that don't already exist in the database (new items have no matching ID)
        if (check.InventoryCheckItems != null && check.InventoryCheckItems.Count > 0)
        {
            // Get existing item IDs from database
            var existingItemIds = await _context.InventoryCheckItems
                .Where(i => i.CheckId == check.Id)
                .Select(i => i.Id)
                .ToListAsync();

            // Only add truly new items
            foreach (var item in check.InventoryCheckItems)
            {
                if (!existingItemIds.Contains(item.Id))
                {
                    _context.InventoryCheckItems.Add(item);
                }
            }
        }

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
