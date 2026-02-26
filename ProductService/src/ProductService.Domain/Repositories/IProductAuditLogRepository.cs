using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories;

public interface IProductAuditLogRepository
{
    Task<ProductAuditLog> CreateAsync(ProductAuditLog auditLog);
    Task<IEnumerable<ProductAuditLog>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<ProductAuditLog>> GetByUserIdAsync(Guid userId);
}
