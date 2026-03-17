using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchRepository
{
    Task<ProductBatch> AddAsync(ProductBatch batch);
    Task<IEnumerable<ProductBatch>> GetAllAsync();
    Task<ProductBatch?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductBatch>> GetByWarehouseIdAsync(Guid warehouseId);
    Task UpdateAsync(ProductBatch batch);
}