using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class WarehouseSlotRepository : IWarehouseSlotRepository
{
    private readonly InventoryDbContext _context;

    public WarehouseSlotRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<WarehouseSlot?> GetByIdAsync(Guid id)
    {
        return await _context.WarehouseSlots
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<WarehouseSlot>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        return await _context.WarehouseSlots
            .Where(s => s.WarehouseId == warehouseId && !s.IsDeleted)
            .OrderBy(s => s.SlotCode)
            .ToListAsync();
    }

    public async Task<bool> ExistsSlotCodeAsync(Guid warehouseId, string slotCode, Guid? excludeId = null)
    {
        return await _context.WarehouseSlots
            .AnyAsync(s => s.WarehouseId == warehouseId
                        && s.SlotCode == slotCode
                        && !s.IsDeleted
                        && (excludeId == null || s.Id != excludeId));
    }

    public async Task<WarehouseSlot> AddAsync(WarehouseSlot slot)
    {
        slot.CreatedAt = DateTime.UtcNow;
        _context.WarehouseSlots.Add(slot);
        await _context.SaveChangesAsync();
        return slot;
    }

    public async Task UpdateAsync(WarehouseSlot slot)
    {
        _context.WarehouseSlots.Update(slot);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var slot = await _context.WarehouseSlots.FindAsync(id);
        if (slot != null)
        {
            slot.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
