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
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Inventory?> GetByIdAsync(Guid id)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Inventory>> GetByStoreIdAsync(Guid storeId)
    {
        return await _context.Inventories
            .Where(i => i.StoreId == storeId)
            .OrderBy(i => i.ProductId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventory>> GetByProductIdAsync(Guid productId)
    {
        return await _context.Inventories
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.StoreId)
            .ToListAsync();
    }

    public async Task<Inventory?> GetByStoreAndProductAsync(Guid storeId, Guid productId)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.ProductId == productId);
    }

    public async Task<Inventory> AddAsync(Inventory inventory)
    {
        inventory.CreatedAt = DateTime.UtcNow;
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
