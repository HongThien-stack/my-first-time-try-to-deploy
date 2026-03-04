using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchService
{
    Task<IEnumerable<ProductBatchDto>> GetAllAsync();
}