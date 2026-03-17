using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class RestockRequestService : IRestockRequestService
{
    private readonly IRestockRequestRepository _requestRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductBatchRepository _productBatchRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<RestockRequestService> _logger;

    public RestockRequestService(
        IRestockRequestRepository requestRepository,
        ITransferRepository transferRepository,
        IStockMovementRepository stockMovementRepository,
        IProductBatchRepository productBatchRepository,
        IProductServiceClient productServiceClient,
        ILogger<RestockRequestService> logger)
    {
        _requestRepository = requestRepository;
        _transferRepository = transferRepository;
        _stockMovementRepository = stockMovementRepository;
        _productBatchRepository = productBatchRepository;
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

    public async Task<RestockRequest?> GetByRequestIdWithoutRestockRequestItemAsync(Guid restockRequestId)
    {
        return await _requestRepository.GetByRequestIdWithoutRestockRequestItemAsync(restockRequestId);
    }

    public async Task<IEnumerable<RestockRequestDto>> GetRequestsByWarehouseAsync(Guid warehouseId)
    {
        _logger.LogInformation("Getting restock requests for warehouse: {WarehouseId}", warehouseId);
        var requests = await _requestRepository.GetByWarehouseIdAsync(warehouseId);
        return requests.Select(MapToDto);
    }

    public async Task<IEnumerable<RestockRequestDto>> GetRequestsByParentWarehouseAsync(Guid parentWarehouseId)
    {
        _logger.LogInformation("Getting restock requests for parent warehouse: {ParentWarehouseId}", parentWarehouseId);
        var requests = await _requestRepository.GetByParentWarehouseIdAsync(parentWarehouseId);
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
        _logger.LogInformation("Creating new restock request to warehouse: {ToWarehouseId}", dto.ToWarehouseId);

        ValidateCreateRequest(dto);

        var requestNumber = await GenerateRequestNumberAsync();

        var request = new RestockRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber,
            FromWarehouseId = dto.FromWarehouseId,
            FromLocationType = dto.FromLocationType.Trim().ToUpperInvariant(),
            ToWarehouseId = dto.ToWarehouseId,
            ToLocationType = dto.ToLocationType.Trim().ToUpperInvariant(),
            RequestedBy = requestedBy,
            RequestedDate = DateTime.UtcNow,
            Priority = string.IsNullOrWhiteSpace(dto.Priority) ? "NORMAL" : dto.Priority.Trim().ToUpperInvariant(),
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

    public async Task<RestockRequestDto> ApproveRequestAsync(Guid id, Guid approvedBy)
    {
        _logger.LogInformation("Approving restock request: {RequestId}", id);

        var request = await _requestRepository.GetByIdAsync(id);
        if (request == null)
            throw new KeyNotFoundException($"Restock request {id} not found");

        if (request.Status != "PENDING")
            throw new InvalidOperationException($"Cannot approve a request with status '{request.Status}'");

        request.Status = "APPROVED";
        request.ApprovedBy = approvedBy;
        request.ApprovedDate = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        await _requestRepository.UpdateAsync(request);

        return MapToDto(request);
    }

    public async Task RejectRequestAsync(Guid id, Guid rejectedBy, RejectRestockRequestDto dto)
    {
        _logger.LogInformation("Rejecting restock request: {RequestId}", id);

        var request = await _requestRepository.GetByIdAsync(id);
        if (request == null)
            throw new KeyNotFoundException($"Restock request {id} not found");

        if (request.Status != "PENDING")
            throw new InvalidOperationException($"Cannot reject a request with status '{request.Status}'");

        request.Status = "REJECTED";
        request.ApprovedBy = rejectedBy;
        request.ApprovedDate = DateTime.UtcNow;
        request.Notes = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Rejection reason: {dto.Reason}"
            : $"{request.Notes}\nRejection reason: {dto.Reason}";
        request.UpdatedAt = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request);
    }

    public async Task<bool> DeleteRequestAsync(Guid id)
    {
        _logger.LogInformation("Deleting restock request: {RequestId}", id);
        await _requestRepository.DeleteAsync(id);
        return true;
    }

    public async Task UpdateAsync(RestockRequest request)
    {
        _logger.LogInformation("Updating restock request: {RequestId}", request.Id);
        await _requestRepository.UpdateAsync(request);
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
            FromWarehouseId = request.FromWarehouseId,
            FromLocationType = request.FromLocationType,
            ToWarehouseId = request.ToWarehouseId,
            ToLocationType = request.ToLocationType,
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
        var count = allRequests.Count(r => r.RequestNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }

    private static void ValidateCreateRequest(CreateRestockRequestDto dto)
    {
        if (dto.ToWarehouseId == null || dto.ToWarehouseId == Guid.Empty)
            throw new InvalidOperationException("ToWarehouseId is required.");

        if (string.IsNullOrWhiteSpace(dto.ToLocationType))
            throw new InvalidOperationException("ToLocationType is required.");

        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException("At least one restock request item is required.");

        var validLocationTypes = new[] { "WAREHOUSE", "STORE" };
        if (!validLocationTypes.Contains(dto.ToLocationType.Trim().ToUpperInvariant()))
            throw new InvalidOperationException("ToLocationType must be WAREHOUSE or STORE.");

        if (!string.IsNullOrWhiteSpace(dto.FromLocationType) && !validLocationTypes.Contains(dto.FromLocationType.Trim().ToUpperInvariant()))
            throw new InvalidOperationException("FromLocationType must be WAREHOUSE or STORE.");

        if (dto.FromWarehouseId.HasValue && dto.FromWarehouseId.Value != Guid.Empty && dto.FromWarehouseId == dto.ToWarehouseId)
            throw new InvalidOperationException("Source and destination locations must be different.");

        if (dto.Items.Any(item => item.ProductId == Guid.Empty))
            throw new InvalidOperationException("Each restock request item must have a valid ProductId.");

        if (dto.Items.Any(item => item.RequestedQuantity <= 0))
            throw new InvalidOperationException("Each restock request item must have RequestedQuantity greater than 0.");

        if (dto.Items.Any(item => item.CurrentQuantity < 0))
            throw new InvalidOperationException("CurrentQuantity cannot be negative.");
    }

    private async Task<string> GenerateTransferNumberAsync()
    {
        var now = DateTime.UtcNow;
        var prefix = $"TRF-{now:yyyy}-";
        var all = await _transferRepository.GetAllAsync();
        var count = all.Count(t => t.TransferNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }

    private async Task<string> GenerateMovementNumberAsync()
    {
        var now = DateTime.UtcNow;
        var prefix = $"SM-{now:yyyy}-";
        var all = await _stockMovementRepository.GetAllAsync();
        var count = all.Count(m => m.MovementNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }
}
