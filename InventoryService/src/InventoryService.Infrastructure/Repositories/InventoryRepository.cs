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

    public async Task<Inventory?> GetInventoryByLocationIdAndProductIdAsync(Guid deliverWarehouseId, Guid productId)
    {
         return await _context.Inventories
            .FirstOrDefaultAsync(i => i.LocationId == deliverWarehouseId && i.ProductId == productId);
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
