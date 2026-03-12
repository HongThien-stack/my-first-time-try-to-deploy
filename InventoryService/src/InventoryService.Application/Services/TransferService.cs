using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.Xml;

namespace InventoryService.Application.Services;

public class TransferService : ITransferService
{
    private readonly ITransferRepository _transferRepository;
    private readonly IProductBatchRepository _productBatchRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IRestockRequestRepository _restockRequestRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<TransferService> _logger;

    public TransferService(
        ITransferRepository transferRepository,
        IProductBatchRepository productBatchRepository,
        IStockMovementRepository stockMovementRepository,
        IRestockRequestRepository restockRequestRepository,
        IInventoryRepository inventoryRepository,
        ILogger<TransferService> logger)
    {
        _transferRepository = transferRepository;
        _productBatchRepository = productBatchRepository;
        _stockMovementRepository = stockMovementRepository;
        _restockRequestRepository = restockRequestRepository;
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TransferDto>> GetAllTransfersAsync()
    {
        _logger.LogInformation("Getting all transfers");
        var transfers = await _transferRepository.GetAllAsync();
        return transfers.Select(MapToDto);
    }

    public async Task<TransferDto?> GetTransferByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting transfer by ID: {TransferId}", id);
        var transfer = await _transferRepository.GetByIdAsync(id);
        return transfer != null ? MapToDto(transfer) : null;
    }

    public async Task<TransferDto?> GetTransferByNumberAsync(string transferNumber)
    {
        _logger.LogInformation("Getting transfer by number: {TransferNumber}", transferNumber);
        var transfer = await _transferRepository.GetByTransferNumberAsync(transferNumber);
        return transfer != null ? MapToDto(transfer) : null;
    }

    public async Task<IEnumerable<TransferDto>> GetTransfersByStatusAsync(string status)
    {
        _logger.LogInformation("Getting transfers by status: {Status}", status);
        var transfers = await _transferRepository.GetByStatusAsync(status);
        return transfers.Select(MapToDto);
    }

    public async Task<TransferDto> CreateTransferAsync(CreateTransferDto dto)
    {
        _logger.LogInformation("Creating new transfer from {FromType}:{FromId} to {ToType}:{ToId}",
            dto.FromLocationType, dto.FromLocationId, dto.ToLocationType, dto.ToLocationId);

        var transferNumber = await GenerateTransferNumberAsync();

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            TransferNumber = transferNumber,
            FromLocationType = dto.FromLocationType,
            FromLocationId = dto.FromLocationId,
            ToLocationType = dto.ToLocationType,
            ToLocationId = dto.ToLocationId,
            TransferDate = DateTime.UtcNow,
            ExpectedDelivery = dto.ExpectedDelivery,
            Status = "PENDING",
            ShippedBy = dto.ShippedBy,
            RestockRequestId = dto.RestockRequestId,
            Notes = dto.Notes,
            TransferItems = dto.Items.Select(item => new TransferItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                BatchId = item.BatchId,
                RequestedQuantity = item.RequestedQuantity
            }).ToList()
        };

        var created = await _transferRepository.AddAsync(transfer);
        return MapToDto(created);
    }

    public async Task UpdateTransferStatusAsync(Guid id, string status, Guid? userId = null)
    {
        _logger.LogInformation("Updating transfer {TransferId} status to {Status}", id, status);

        var transfer = await _transferRepository.GetByIdAsync(id);
        if (transfer == null)
        {
            throw new KeyNotFoundException($"Transfer {id} not found");
        }

        transfer.Status = status;

        if (status == "IN_TRANSIT" && userId.HasValue)
        {
            transfer.ShippedBy = userId.Value;
        }
        else if (status == "DELIVERED" && userId.HasValue)
        {
            transfer.ReceivedBy = userId.Value;
            transfer.ActualDelivery = DateTime.UtcNow;
        }

        await _transferRepository.UpdateAsync(transfer);
    }

    public async Task<bool> DeleteTransferAsync(Guid id)
    {
        _logger.LogInformation("Deleting transfer: {TransferId}", id);
        await _transferRepository.DeleteAsync(id);
        return true;
    }

    public async Task<TransferDto> ReceiveTransferAsync(Guid transferId, ReceiveTransferDto dto, Guid receivedBy)
    {
        _logger.LogInformation("Receiving transfer {TransferId} by user {UserId}", transferId, receivedBy);
        //1. Load transfer
        var transfer = await _transferRepository.GetByIdAsync(transferId)
            ?? throw new KeyNotFoundException($"Transfer {transferId} not found");

        if (transfer.Status == "COMPLETED" || transfer.Status == "CANCELLED")
            throw new InvalidOperationException($"Transfer is already {transfer.Status} and cannot be received");

        //2. Update transfer item
        var itemLookup = dto.Items.ToDictionary(i => i.TransferItemId);
        var stockMovementItems = new List<StockMovementItem>();

        foreach(var transferItem in transfer.TransferItems)
        {
            if (!itemLookup.TryGetValue(transferItem.Id, out var incoming))
                continue;
            if (incoming.ShippedQuantity < 0 || incoming.DamagedQuantity < 0)
                throw new InvalidOperationException($"Quantities for item {transferItem.Id} cannot be negative");

            if (incoming.DamagedQuantity > incoming.ShippedQuantity)
                throw new InvalidOperationException($"DamagedQuantity cannot exceed ShippedQuantity for item {transferItem.Id}");


            int receivedQuantity = incoming.ShippedQuantity - incoming.DamagedQuantity;

            // 2a. Ghi nhận số lượng thực nhận lên TransferItem
            transferItem.ShippedQuantity = incoming.ShippedQuantity;
            transferItem.ReceivedQuantity = receivedQuantity;  // ← Tự tính
            transferItem.DamagedQuantity = incoming.DamagedQuantity;
            transferItem.Notes = incoming.Notes ?? transferItem.Notes;

            // 3. Cộng hàng tốt vào inventories kho đích (ToLocation)
            //    netReceived = received - damaged (hàng thực sự nhập kho)
            if (receivedQuantity > 0)
            {
                var destInventory = await _inventoryRepository
                    .GetByLocationAndProductAsync(
                    transfer.ToLocationType,
                    transfer.ToLocationId,
                    transferItem.ProductId);
                if (destInventory != null)
                {
                    // Cộng vào quantity — AvailableQuantity tự tính (computed = quantity - reserved)
                    destInventory.Quantity += receivedQuantity;
                    destInventory.UpdatedAt = DateTime.UtcNow;
                    await _inventoryRepository.UpdateAsync(destInventory);
                }
                else
                {
                    // Tạo mới bản ghi tồn kho nếu sản phẩm chưa có tại kho đích
                    await _inventoryRepository.AddAsync(new Inventory()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = transferItem.ProductId,
                        LocationId = transfer.ToLocationId,
                        LocationType = transfer.ToLocationType,
                        Quantity = receivedQuantity,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            // 4. Gom StockMovementItem — quantity = netReceived (đã trừ hàng hư)
            stockMovementItems.Add(new StockMovementItem
            {
                Id = Guid.NewGuid(),
                ProductId = transferItem.ProductId,
                Quantity = receivedQuantity
            });
        } // kết thúc foreach

        // 5. Tạo StockMovement INBOUND ghi nhận tại kho đích
        var movementCount = await _stockMovementRepository.CountByDateAsync(DateTime.UtcNow);
        var stockMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = $"SM-{DateTime.UtcNow:yyyyMMdd}-{(movementCount + 1):D3}",
            MovementType = "INBOUND",
            LocationId = transfer.ToLocationId,
            LocationType = transfer.ToLocationType,
            MovementDate = DateTime.UtcNow,
            TransferId = transfer.Id,
            RestockRequestId = transfer.RestockRequestId,
            ReceivedBy = receivedBy,
            Status = "COMPLETED",
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            StockMovementItems = stockMovementItems,
        };
        await _stockMovementRepository.AddAsync(stockMovement);

        // 6. Cập nhật Transfer → COMPLETED
        transfer.Status = "COMPLETED";
        transfer.ReceivedBy = receivedBy;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.ActualDelivery = DateTime.UtcNow;
        await _transferRepository.UpdateAsync(transfer);

        // 7. Cập nhật RestockRequest → COMPLETED nếu có liên kết
        if (transfer.RestockRequestId.HasValue)
        {
            var restockRequest = await _restockRequestRepository.GetByIdAsync(transfer.RestockRequestId.Value);
            if (restockRequest != null)
            {
                restockRequest.Status = "COMPLETED";
                restockRequest.UpdatedAt = DateTime.UtcNow;
                await _restockRequestRepository.UpdateAsync(restockRequest);
                _logger.LogInformation("RestockRequest {RequestId} marked as COMPLETED", restockRequest.Id);

            }
        }
        _logger.LogInformation("Transfer {TransferId} received and marked COMPLETED", transferId);
        return MapToDto(transfer);
    }




    private TransferDto MapToDto(Transfer transfer)
    {
        return new TransferDto
        {
            Id = transfer.Id,
            TransferNumber = transfer.TransferNumber,
            FromLocationType = transfer.FromLocationType,
            FromLocationId = transfer.FromLocationId,
            ToLocationType = transfer.ToLocationType,
            ToLocationId = transfer.ToLocationId,
            TransferDate = transfer.TransferDate,
            ExpectedDelivery = transfer.ExpectedDelivery,
            ActualDelivery = transfer.ActualDelivery,
            Status = transfer.Status,
            ShippedBy = transfer.ShippedBy,
            ReceivedBy = transfer.ReceivedBy,
            RestockRequestId = transfer.RestockRequestId,
            Notes = transfer.Notes,
            Items = transfer.TransferItems.Select(item => new TransferItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                BatchId = item.BatchId,
                RequestedQuantity = item.RequestedQuantity,
                ShippedQuantity = item.ShippedQuantity,
                ReceivedQuantity = item.ReceivedQuantity,
                DamagedQuantity = item.DamagedQuantity,
                Notes = item.Notes
            }).ToList()
        };
    }

    private async Task<string> GenerateTransferNumberAsync()
    {
        var now = DateTime.UtcNow;
        var prefix = $"TRF-{now:yyyy}-";
        var allTransfers = await _transferRepository.GetAllAsync();
        var todayTransfers = allTransfers.Where(t => t.TransferNumber.StartsWith(prefix)).Count();
        return $"{prefix}{(todayTransfers + 1):D3}";
    }
}
