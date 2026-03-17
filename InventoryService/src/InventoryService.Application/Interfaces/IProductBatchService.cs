using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IProductBatchService
{
    Task<IEnumerable<ProductBatchDto>> GetAllAsync();
    Task<ProductBatchDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductBatchDto>> GetByWarehouseIdAsync(Guid warehouseId);
    Task<ProductBatchDto> AllocateBatchAsync(CreateAllocatedBatchDto dto);
    Task<IEnumerable<ProductBatchDto>> ReceiveFromSupplierAsync(ReceiveFromSupplierDto dto);
    
    /// <summary>
    /// Automatically update batch status to EXPIRED based on expiry date
    /// </summary>
    Task<int> UpdateExpiredBatchesAsync();

    /// <summary>
    /// Create outbound stock movement for expired batches at a warehouse
    /// </summary>
    Task<OutboundStockMovementResponseDto> CreateOutboundFromExpiredBatchesAsync(CreateOutboundFromExpiredBatchesDto request);
}