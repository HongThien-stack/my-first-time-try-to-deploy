using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly InventoryDbContext _context;

    public WarehouseRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _context.Warehouses
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<List<Warehouse>> GetAllWarehouseByParentIdAsync(Guid parentId)
    {
        return await _context.Warehouses
            .Where(w => w.ParentId == parentId && !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async Task AddWarehouseAsync(Warehouse warehouse)
    {
        await _context.Warehouses.AddAsync(warehouse);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateWarehouseAsync(Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteWarehouseAsync(Guid id)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse != null)
        {
            warehouse.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
