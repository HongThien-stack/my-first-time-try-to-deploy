using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchRepository
{
    Task<IEnumerable<ProductBatch>> GetAllAsync();
    Task<ProductBatch?> GetByIdAsync(Guid id);
    Task<ProductBatch> AddAsync(ProductBatch batch);
    Task UpdateAsync(ProductBatch batch);
}