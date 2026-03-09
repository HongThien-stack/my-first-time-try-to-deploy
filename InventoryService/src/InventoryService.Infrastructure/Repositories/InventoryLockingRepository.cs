using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class InventoryLockingRepository : IInventoryLockingRepository
{
    private readonly InventoryDbContext _context;

    public InventoryLockingRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByLocationAndProductWithLockAsync(
        string locationType, 
        Guid locationId, 
        Guid productId)
    {
        // Simplified: database only has store_id (no location_type/location_id)
        // Use raw SQL to apply UPDLOCK and ROWLOCK hints
        var inventory = await _context.Inventories
            .FromSqlRaw(@"
                SELECT * FROM inventories WITH (UPDLOCK, ROWLOCK)
                WHERE store_id = {0} 
                AND product_id = {1}",
                locationId, productId)
            .FirstOrDefaultAsync();

        return inventory;
    }

    public void UpdateInventoryInTransaction(Inventory inventory)
    {
        // Mark entity as modified without saving
        _context.Inventories.Update(inventory);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await operation();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
