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

        var requestNumber = await GenerateRequestNumberAsync();
        
        var request = new RestockRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber,
            FromWarehouseId = dto.FromWarehouseId,
            FromLocationType = dto.FromLocationType,
            ToWarehouseId = dto.ToWarehouseId,
            ToLocationType = dto.ToLocationType,
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

    public async Task<ApproveRestockResponseDto> ApproveRequestAsync(Guid id, Guid approvedBy, ApproveRestockRequestDto dto)
    {
        _logger.LogInformation("Approving restock request: {RequestId}", id);

        var request = await _requestRepository.GetByIdAsync(id);
        if (request == null)
            throw new KeyNotFoundException($"Restock request {id} not found");

        if (request.Status != "PENDING")
            throw new InvalidOperationException($"Cannot approve a request with status '{request.Status}'");

        // Build a lookup: restockItemId -> approve detail
        var approveMap = dto.Items.ToDictionary(i => i.RestockItemId);

        // Apply approved quantities to each restock item
        var items = request.RestockRequestItems.ToList();
        foreach (var item in items)
        {
            if (approveMap.TryGetValue(item.Id, out var detail))
                item.ApprovedQuantity = detail.ApprovedQuantity;
        }

        // ── 1. Create Transfer (only when both source and destination are internal) ──
        Transfer? createdTransfer = null;
        if (request.FromWarehouseId.HasValue && request.ToWarehouseId.HasValue)
        {
            var transferNumber = await GenerateTransferNumberAsync();
            var transfer = new Transfer
            {
                Id = Guid.NewGuid(),
                TransferNumber = transferNumber,
                FromLocationType = request.FromLocationType,
                FromLocationId = request.FromWarehouseId.Value,
                ToLocationType = request.ToLocationType,
                ToLocationId = request.ToWarehouseId.Value,
                TransferDate = DateTime.UtcNow,
                ExpectedDelivery = dto.ExpectedDelivery,
                Status = "PENDING",
                ShippedBy = approvedBy,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                TransferItems = items.Select(item =>
                {
                    approveMap.TryGetValue(item.Id, out var detail);
                    return new TransferItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        RequestedQuantity = detail?.ApprovedQuantity ?? item.RequestedQuantity,
                        Notes = dto.Notes
                    };
                }).ToList()
            };
            createdTransfer = await _transferRepository.AddAsync(transfer);
        }

        // ── 2. StockMovement – OUTBOUND at source (if internal transfer) or INBOUND at destination (if supplier) ─
        var movementLocation = request.ToWarehouseId ?? request.FromWarehouseId;
        var movementLocationType = request.ToWarehouseId.HasValue ? request.ToLocationType : request.FromLocationType;
        var movementNumber = await GenerateMovementNumberAsync();
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = movementNumber,
            MovementType = request.FromWarehouseId.HasValue ? "OUTBOUND" : "INBOUND",
            LocationId = movementLocation!.Value,
            LocationType = movementLocationType,
            MovementDate = DateTime.UtcNow,
            TransferId = createdTransfer?.Id,
            RestockRequestId = request.Id,
            ReceivedBy = approvedBy,
            Status = "PENDING",
            Notes = $"Restock {(request.FromWarehouseId.HasValue ? "outbound" : "inbound")} for request {request.RequestNumber}",
            CreatedAt = DateTime.UtcNow,
            StockMovementItems = items.Select(item =>
            {
                approveMap.TryGetValue(item.Id, out var detail);
                return new StockMovementItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = detail?.ApprovedQuantity ?? item.RequestedQuantity,
                    UnitPrice = detail?.UnitPrice
                };
            }).ToList()
        };
        var createdMovement = await _stockMovementRepository.AddAsync(movement);

        // ── 3. Link restock request ID into each referenced ProductBatch ──────────────────────────────────
        var batchIds = dto.Items
            .Where(i => i.BatchId.HasValue)
            .Select(i => i.BatchId!.Value)
            .Distinct();
        foreach (var batchId in batchIds)
        {
            var batch = await _productBatchRepository.GetByIdAsync(batchId);
            if (batch != null)
            {
                batch.RestockRequestId = request.Id;
                await _productBatchRepository.UpdateAsync(batch);
            }
        }

        // ── 5. Update RestockRequest status ──────────────────────────────────
        request.Status = "APPROVED";
        request.ApprovedBy = approvedBy;
        request.ApprovedDate = DateTime.UtcNow;
        request.TransferId = createdTransfer?.Id;
        request.UpdatedAt = DateTime.UtcNow;
        await _requestRepository.UpdateAsync(request);

        // ── 6. Enrich movement items with product name/unit ───────────────────
        var productIds = createdMovement.StockMovementItems.Select(i => i.ProductId).Distinct();
        var productMap = await _productServiceClient.GetProductsByIdsAsync(productIds);

        var movementDto = new StockMovementDto
        {
            Id = createdMovement.Id,
            MovementNumber = createdMovement.MovementNumber,
            MovementType = createdMovement.MovementType,
            LocationId = createdMovement.LocationId,
            LocationType = createdMovement.LocationType,
            MovementDate = createdMovement.MovementDate,
            RestockRequestId = createdMovement.RestockRequestId,
            TransferId = createdMovement.TransferId,
            ReceivedBy = createdMovement.ReceivedBy,
            Status = createdMovement.Status,
            Notes = createdMovement.Notes,
            CreatedAt = createdMovement.CreatedAt,
            TotalItems = createdMovement.StockMovementItems.Count,
            Items = createdMovement.StockMovementItems.Select(i =>
            {
                productMap.TryGetValue(i.ProductId, out var info);
                return new StockMovementItemDto
                {
                    Id = i.Id,
                    MovementId = createdMovement.Id,
                    ProductId = i.ProductId,
                    ProductName = info?.Name,
                    Unit = info?.Unit,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                };
            }).ToList()
        };

        return new ApproveRestockResponseDto
        {
            RestockRequest = MapToDto(request),
            StockMovement = movementDto
        };
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
