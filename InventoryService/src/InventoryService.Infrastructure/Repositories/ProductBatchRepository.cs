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

    public async Task<IEnumerable<ProductBatch>> GetAllAsync()
    {
        return await _context.ProductBatches
            .OrderByDescending(b => b.ReceivedAt)
            .ToListAsync();
    }
}