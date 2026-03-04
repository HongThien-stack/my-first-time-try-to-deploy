using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchRepository
{
    Task<IEnumerable<ProductBatch>> GetAllAsync();
}