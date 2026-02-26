using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductAuditLogRepository : IProductAuditLogRepository
{
    private readonly ProductDbContext _context;

    public ProductAuditLogRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<ProductAuditLog> CreateAsync(ProductAuditLog auditLog)
    {
        await _context.ProductAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
        return auditLog;
    }

    public async Task<IEnumerable<ProductAuditLog>> GetByProductIdAsync(Guid productId)
    {
        return await _context.ProductAuditLogs
            .Where(log => log.ProductId == productId)
            .OrderByDescending(log => log.PerformedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductAuditLog>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ProductAuditLogs
            .Where(log => log.PerformedBy == userId)
            .OrderByDescending(log => log.PerformedAt)
            .ToListAsync();
    }
}
