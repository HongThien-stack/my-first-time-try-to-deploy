using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces;

public interface IRestockRequestService
{
    Task<IEnumerable<RestockRequestDto>> GetAllRequestsAsync();
    Task<RestockRequestDto?> GetRequestByIdAsync(Guid id);
    Task<IEnumerable<RestockRequestDto>> GetRequestsByStoreAsync(Guid storeId);
    Task<IEnumerable<RestockRequestDto>> GetPendingRequestsAsync();
    Task<RestockRequestDto> CreateRequestAsync(CreateRestockRequestDto dto);
    Task ApproveRequestAsync(Guid id, Guid approvedBy, List<int?> approvedQuantities);
    Task RejectRequestAsync(Guid id, Guid rejectedBy, string reason);
    Task<bool> DeleteRequestAsync(Guid id);
}
