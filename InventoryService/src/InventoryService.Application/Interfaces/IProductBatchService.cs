using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchService
{
    Task<IEnumerable<ProductBatchDto>> GetAllAsync();
    Task<ProductBatchDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductBatchDto>> GetByWarehouseIdAsync(Guid warehouseId);
    Task<ProductBatchDto> AllocateBatchAsync(CreateAllocatedBatchDto dto);
    Task<IEnumerable<ProductBatchDto>> ReceiveFromSupplierAsync(ReceiveFromSupplierDto dto); // <-- Add this
}