using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IRestockRequestRepository
{
    Task<IEnumerable<RestockRequest>> GetAllAsync();
    Task<RestockRequest?> GetByIdAsync(Guid id);
    Task<RestockRequest?> GetByRequestNumberAsync(string requestNumber);
    Task<IEnumerable<RestockRequest>> GetByStoreIdAsync(Guid storeId);
    Task<IEnumerable<RestockRequest>> GetByStatusAsync(string status);
    Task<IEnumerable<RestockRequest>> GetByPriorityAsync(string priority);
    Task<IEnumerable<RestockRequest>> GetPendingRequestsAsync();
    Task<RestockRequest> AddAsync(RestockRequest request);
    Task UpdateAsync(RestockRequest request);
    Task DeleteAsync(Guid id);
}
