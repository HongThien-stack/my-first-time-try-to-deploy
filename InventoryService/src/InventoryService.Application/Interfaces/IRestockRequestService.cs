using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IRestockRequestService
{
    Task<IEnumerable<RestockRequestDto>> GetAllRequestsAsync();
    Task<RestockRequestDto?> GetRequestByIdAsync(Guid id);
    Task<RestockRequest?> GetByRequestIdWithoutRestockRequestItemAsync(Guid restockRequestId);
    /// <summary>Get requests where from_warehouse_id or to_warehouse_id equals the given warehouse.</summary>
    Task<IEnumerable<RestockRequestDto>> GetRequestsByWarehouseAsync(Guid warehouseId);
    /// <summary>Get all requests whose from or to warehouse belongs to the given parent warehouse.</summary>
    Task<IEnumerable<RestockRequestDto>> GetRequestsByParentWarehouseAsync(Guid parentWarehouseId);
    Task<IEnumerable<RestockRequestDto>> GetPendingRequestsAsync();
    Task<RestockRequestDto> CreateRequestAsync(CreateRestockRequestDto dto, Guid requestedBy);
    Task<RestockRequestDto> ApproveRequestAsync(Guid id, Guid approvedBy);
    Task RejectRequestAsync(Guid id, Guid rejectedBy, RejectRestockRequestDto dto);
    Task UpdateAsync(RestockRequest request);
    Task<bool> DeleteRequestAsync(Guid id);
}
