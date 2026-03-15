using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IRestockRequestRepository
{
    Task<IEnumerable<RestockRequest>> GetAllAsync();
    Task<RestockRequest?> GetByIdAsync(Guid id);
    Task<RestockRequest?> GetByRequestIdWithoutRestockRequestItemAsync(Guid restockRequestId);
    Task<RestockRequest?> GetByRequestNumberAsync(string requestNumber);
    Task<IEnumerable<RestockRequest>> GetByWarehouseIdAsync(Guid warehouseId);
    Task<IEnumerable<RestockRequest>> GetByParentWarehouseIdAsync(Guid parentWarehouseId);
    Task<IEnumerable<RestockRequest>> GetByStatusAsync(string status);
    Task<IEnumerable<RestockRequest>> GetByPriorityAsync(string priority);
    Task<IEnumerable<RestockRequest>> GetPendingRequestsAsync();
    Task<RestockRequest> AddAsync(RestockRequest request);
    Task UpdateAsync(RestockRequest request);
    Task DeleteAsync(Guid id);
}
