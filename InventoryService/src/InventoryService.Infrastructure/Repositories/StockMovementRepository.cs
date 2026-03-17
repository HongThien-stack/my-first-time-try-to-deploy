using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly InventoryDbContext _context;

    public StockMovementRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockMovement>> GetAllAsync()
    {
        return await _context.StockMovements
            .Include(sm => sm.StockMovementItems)
            .OrderByDescending(sm => sm.CreatedAt)
            .ToListAsync();
    }

    public async Task<StockMovement?> GetByIdAsync(Guid id)
    {
        return await _context.StockMovements
            .Include(sm => sm.StockMovementItems)
            .FirstOrDefaultAsync(sm => sm.Id == id);
    }

    public async Task<StockMovement?> GetByMovementNumberAsync(string movementNumber)
    {
        return await _context.StockMovements
            .Include(sm => sm.StockMovementItems)
            .FirstOrDefaultAsync(sm => sm.MovementNumber == movementNumber);
    }

    public async Task<IEnumerable<StockMovement>> GetByLocationAsync(string locationType, Guid locationId)
    {
        return await _context.StockMovements
            .Include(sm => sm.StockMovementItems)
            .Where(sm => sm.LocationType == locationType && sm.LocationId == locationId)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(string movementType)
    {
        return await _context.StockMovements
            .Include(sm => sm.StockMovementItems)
            .Where(sm => sm.MovementType == movementType)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByLocationIdAsync(Guid locationId)
    {
        return await _context.StockMovements
            .Include(sm => sm.StockMovementItems)
            .Where(sm => sm.LocationId == locationId)
            .OrderByDescending(sm => sm.MovementDate)
            .ToListAsync();
    }

    public async Task<int> CountByDateAsync(DateTime date)
    {
        return await _context.StockMovements 
            .CountAsync(sm => sm.MovementDate.Date == date.Date);
    }

    public async Task<StockMovement> AddAsync(StockMovement movement)
    {
        movement.CreatedAt = DateTime.UtcNow;
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();
        return movement;
    }

    public async Task AddNewStockMovementAsync(StockMovement stockMovement)
    {
        _context.StockMovements.Add(stockMovement);
        await _context.SaveChangesAsync();
    }

    public async Task AddNewStockMovementItemAsync(StockMovementItem stockMovementItem)
    {
        _context.StockMovementItems.Add(stockMovementItem);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountStockMovementAsync()
    {
        return await _context.StockMovements.CountAsync();
    }

    public async Task UpdateAsync(StockMovement movement)
    {
        _context.StockMovements.Update(movement);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var movement = await _context.StockMovements.FindAsync(id);
        if (movement != null)
        {
            _context.StockMovements.Remove(movement);
            await _context.SaveChangesAsync();
        }
    }
}
