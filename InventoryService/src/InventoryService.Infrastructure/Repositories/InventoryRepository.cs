using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Inventory>> GetAllAsync()
    {
        return await _context.Inventories
            .OrderBy(i => i.LocationType)
            .ThenBy(i => i.LocationId)
            .ThenBy(i => i.ProductId)
            .ToListAsync();
    }

    public async Task<Inventory?> GetByIdAsync(Guid id)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Inventory>> GetByLocationAsync(string locationType, Guid locationId)
    {
        return await _context.Inventories
            .Where(i => i.LocationType == locationType && i.LocationId == locationId)
            .OrderBy(i => i.ProductId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventory>> GetByLocationIdAsync(Guid locationId)
    {
        return await _context.Inventories
            .Where(i => i.LocationId == locationId)
            .OrderBy(i => i.LocationType)
            .ThenBy(i => i.ProductId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventory>> GetByProductIdAsync(Guid productId)
    {
        return await _context.Inventories
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.LocationType)
            .ThenBy(i => i.LocationId)
            .ToListAsync();
    }

    public async Task<Inventory?> GetByLocationAndProductAsync(string locationType, Guid locationId, Guid productId)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(i => i.LocationType == locationType && i.LocationId == locationId && i.ProductId == productId);
    }

    public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync(string? locationType = null)
    {
        var query = _context.Inventories
            .Where(i => i.AvailableQuantity <= i.MinStockLevel);
        
        if (!string.IsNullOrEmpty(locationType))
        {
            query = query.Where(i => i.LocationType == locationType);
        }
        
        return await query
            .OrderBy(i => i.LocationType)
            .ThenBy(i => i.LocationId)
            .ThenBy(i => i.ProductId)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Inventory> Items, int TotalCount)> GetLowStockAlertsAsync(
        string? locationType = null,
        Guid? locationId = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        // Build base query: available_quantity <= min_stock_level
        // Note: available_quantity is computed as (quantity - reserved_quantity)
        var query = _context.Inventories
            .Where(i => i.MinStockLevel.HasValue 
                && (i.Quantity - i.ReservedQuantity) <= i.MinStockLevel.Value);

        // Apply filters
        if (!string.IsNullOrEmpty(locationType))
        {
            query = query.Where(i => i.LocationType == locationType);
        }

        if (locationId.HasValue)
        {
            query = query.Where(i => i.LocationId == locationId.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting: lowest stock first (critical items at top)
        // Sort by computed available quantity (quantity - reserved_quantity)
        var items = await query
            .OrderBy(i => i.Quantity - i.ReservedQuantity)
            .ThenBy(i => i.ProductId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Inventory> AddAsync(Inventory inventory)
    {
        inventory.UpdatedAt = DateTime.UtcNow;
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task UpdateAsync(Inventory inventory)
    {
        inventory.UpdatedAt = DateTime.UtcNow;
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateReservedQuantityAsync(Inventory inventory)
    {
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory != null)
        {
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }
    }
}
