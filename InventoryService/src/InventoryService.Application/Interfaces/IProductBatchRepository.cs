using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchRepository
{
    Task<ProductBatch> AddAsync(ProductBatch batch);
    Task<IEnumerable<ProductBatch>> GetAllAsync();
    Task<ProductBatch?> GetByIdAsync(Guid id);
<<<<<<< HEAD
    Task<IEnumerable<ProductBatch>> GetByWarehouseIdAsync(Guid warehouseId);
    Task<ProductBatch> AddAsync(ProductBatch batch);
=======
>>>>>>> 4c23c529ee5a6bcbf7313606c207c7c6f98a6854
    Task UpdateAsync(ProductBatch batch);
}