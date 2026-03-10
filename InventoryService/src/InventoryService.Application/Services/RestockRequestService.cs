using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class RestockRequestService : IRestockRequestService
{
    private readonly IRestockRequestRepository _requestRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<RestockRequestService> _logger;

    public RestockRequestService(
        IRestockRequestRepository requestRepository,
        IProductServiceClient productServiceClient,
        ILogger<RestockRequestService> logger)
    {
        _requestRepository = requestRepository;
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<RestockRequestDto>> GetAllRequestsAsync()
    {
        _logger.LogInformation("Getting all restock requests");
        var requests = await _requestRepository.GetAllAsync();
        var dtos = requests.Select(MapToDto).ToList();
        await EnrichWithProductInfoAsync(dtos);
        return dtos;
    }

    public async Task<RestockRequestDto?> GetRequestByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting restock request by ID: {RequestId}", id);
        var request = await _requestRepository.GetByIdAsync(id);
        if (request == null) return null;
        var dto = MapToDto(request);
        await EnrichWithProductInfoAsync(new[] { dto });
        return dto;
    }

    public async Task<IEnumerable<RestockRequestDto>> GetRequestsByStoreAsync(Guid storeId)
    {
        _logger.LogInformation("Getting restock requests for store: {StoreId}", storeId);
        var requests = await _requestRepository.GetByStoreIdAsync(storeId);
        return requests.Select(MapToDto);
    }

    public async Task<IEnumerable<RestockRequestDto>> GetPendingRequestsAsync()
    {
        _logger.LogInformation("Getting pending restock requests");
        var requests = await _requestRepository.GetPendingRequestsAsync();
        return requests.Select(MapToDto);
    }

    public async Task<RestockRequestDto> CreateRequestAsync(CreateRestockRequestDto dto, Guid requestedBy)
    {
        _logger.LogInformation("Creating new restock request for store: {StoreId}", dto.StoreId);

        var requestNumber = await GenerateRequestNumberAsync();
        
        var request = new RestockRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber,
            StoreId = dto.StoreId,
            WarehouseId = dto.WarehouseId,
            RequestedBy = requestedBy,
            RequestedDate = DateTime.UtcNow,
            Priority = dto.Priority,
            Status = "PENDING",
            Notes = dto.Notes,
            RestockRequestItems = dto.Items.Select(item => new RestockRequestItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                RequestedQuantity = item.RequestedQuantity,
                CurrentQuantity = item.CurrentQuantity,
                Reason = item.Reason
            }).ToList()
        };

        var created = await _requestRepository.AddAsync(request);
        return MapToDto(created);
    }

    public async Task ApproveRequestAsync(Guid id, Guid approvedBy, List<int?> approvedQuantities)
    {
        _logger.LogInformation("Approving restock request: {RequestId}", id);
        
        var request = await _requestRepository.GetByIdAsync(id);
        if (request == null)
        {
            throw new KeyNotFoundException($"Restock request {id} not found");
        }

        request.Status = "APPROVED";
        request.ApprovedBy = approvedBy;
        request.ApprovedDate = DateTime.UtcNow;

        // Update approved quantities for each item
        var items = request.RestockRequestItems.ToList();
        for (int i = 0; i < items.Count && i < approvedQuantities.Count; i++)
        {
            items[i].ApprovedQuantity = approvedQuantities[i];
        }

        await _requestRepository.UpdateAsync(request);
    }

    public async Task RejectRequestAsync(Guid id, Guid rejectedBy, string reason)
    {
        _logger.LogInformation("Rejecting restock request: {RequestId}", id);
        
        var request = await _requestRepository.GetByIdAsync(id);
        if (request == null)
        {
            throw new KeyNotFoundException($"Restock request {id} not found");
        }

        request.Status = "REJECTED";
        request.ApprovedBy = rejectedBy;
        request.ApprovedDate = DateTime.UtcNow;
        request.Notes = $"{request.Notes}\nRejection reason: {reason}";

        await _requestRepository.UpdateAsync(request);
    }

    public async Task<bool> DeleteRequestAsync(Guid id)
    {
        _logger.LogInformation("Deleting restock request: {RequestId}", id);
        await _requestRepository.DeleteAsync(id);
        return true;
    }

    private async Task EnrichWithProductInfoAsync(IEnumerable<RestockRequestDto> dtos)
    {
        var productIds = dtos
            .SelectMany(d => d.Items.Select(i => i.ProductId))
            .Distinct();

        var productMap = await _productServiceClient.GetProductsByIdsAsync(productIds);

        foreach (var dto in dtos)
            foreach (var item in dto.Items)
                if (productMap.TryGetValue(item.ProductId, out var info))
                {
                    item.ProductName = info.Name;
                    item.Unit = info.Unit;
                }
    }

    private RestockRequestDto MapToDto(RestockRequest request)
    {
        return new RestockRequestDto
        {
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            StoreId = request.StoreId,
            WarehouseId = request.WarehouseId,
            RequestedBy = request.RequestedBy,
            RequestedDate = request.RequestedDate,
            Priority = request.Priority,
            Status = request.Status,
            ApprovedBy = request.ApprovedBy,
            ApprovedDate = request.ApprovedDate,
            TransferId = request.TransferId,
            Notes = request.Notes,
            Items = request.RestockRequestItems.Select(item => new RestockRequestItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                RequestedQuantity = item.RequestedQuantity,
                CurrentQuantity = item.CurrentQuantity,
                ApprovedQuantity = item.ApprovedQuantity,
                Reason = item.Reason
            }).ToList()
        };
    }

    private async Task<string> GenerateRequestNumberAsync()
    {
        var now = DateTime.UtcNow;
        var prefix = $"RST-{now:yyyy}-";
        var allRequests = await _requestRepository.GetAllAsync();
        var todayRequests = allRequests.Where(r => r.RequestNumber.StartsWith(prefix)).Count();
        return $"{prefix}{(todayRequests + 1):D3}";
    }
}
