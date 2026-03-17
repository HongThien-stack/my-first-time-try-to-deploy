using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchRepository
{
    Task<ProductBatch> AddAsync(ProductBatch batch);
    Task<IEnumerable<ProductBatch>> GetAllAsync();
    Task<ProductBatch?> GetByIdAsync(Guid id);
    Task UpdateAsync(ProductBatch batch);
}