using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class ProductBatchRepository : IProductBatchRepository
{
    private readonly InventoryDbContext _context;

    public ProductBatchRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<ProductBatch> AddAsync(ProductBatch newBatch)
    {
        _context.ProductBatches.Add(newBatch);
        await _context.SaveChangesAsync();
        return newBatch;
    }

    public async Task<IEnumerable<ProductBatch>> GetAllAsync()
    {
        return await _context.ProductBatches
            .OrderByDescending(b => b.ReceivedAt)
            .ToListAsync();
    }

    public async Task<ProductBatch?> GetByIdAsync(Guid id)
    {
        return await _context.ProductBatches.FindAsync(id);
    }

    public async Task UpdateAsync(ProductBatch batch)
    {
        _context.ProductBatches.Update(batch);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductBatch> AddAsync(ProductBatch batch)
    {
        await _context.ProductBatches.AddAsync(batch);
        await _context.SaveChangesAsync();
        return batch;
    }
}