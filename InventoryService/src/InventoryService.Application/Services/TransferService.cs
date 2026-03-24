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
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<TransferService> _logger;

    public TransferService(
        ITransferRepository transferRepository,
        IProductBatchRepository productBatchRepository,
        IStockMovementRepository stockMovementRepository,
        IRestockRequestRepository restockRequestRepository,
        IInventoryRepository inventoryRepository,
        IProductServiceClient productServiceClient,
        ILogger<TransferService> logger)
    {
        _transferRepository = transferRepository;
        _productBatchRepository = productBatchRepository;
        _stockMovementRepository = stockMovementRepository;
        _restockRequestRepository = restockRequestRepository;
        _inventoryRepository = inventoryRepository;
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<TransferDto>> GetAllTransfersAsync()
    {
        _logger.LogInformation("Getting all transfers");
        var transfers = await _transferRepository.GetAllAsync();
        return transfers.Select(MapToDto);
    }

    public async Task<List<Transfer>> GetAllTransfersByFromLocationIdAsync(Guid fromLocationId)
    {
        _logger.LogInformation("Getting all transfers from location ID: {FromLocationId}", fromLocationId);
        return await _transferRepository.GetAllTransfersByFromLocationIdAsync(fromLocationId);
    }

    public async Task<List<TransferItem>> GetAllTransferItemsByTransferIdAsync(Guid transferId)
    {
        _logger.LogInformation("Getting all transfer items for transfer ID: {TransferId}", transferId);
        return await _transferRepository.GetAllTransferItemsByTransferIdAsync(transferId);
    }

    public async Task<TransferDto?> GetTransferByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting transfer by ID: {TransferId}", id);
        var transfer = await _transferRepository.GetByIdAsync(id);
        return transfer != null ? MapToDto(transfer) : null;
    }

    public async Task<Transfer?> GetByTransferIdWithoutTransferItemAsync(Guid transferId)
    {
        _logger.LogInformation("Getting transfer by ID without items: {TransferId}", transferId);
        return await _transferRepository.GetByTransferIdWithoutTransferItemAsync(transferId);
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

        ValidateCreateTransfer(dto);

        var transferNumber = await GenerateTransferNumberAsync();

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            TransferNumber = transferNumber,
            FromLocationType = dto.FromLocationType.Trim().ToUpperInvariant(),
            FromLocationId = dto.FromLocationId,
            ToLocationType = dto.ToLocationType.Trim().ToUpperInvariant(),
            ToLocationId = dto.ToLocationId,
            TransferDate = DateTime.UtcNow,
            ExpectedDelivery = dto.ExpectedDelivery,
            Status = "PENDING",
            ShippedBy = dto.ShippedBy == Guid.Empty ? null : dto.ShippedBy,
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

        // Cập nhật reserved_quantity tại kho nguồn cho từng sản phẩm
        foreach (var item in created.TransferItems)
        {
            var inventory = await _inventoryRepository.GetByLocationAndProductAsync(
                dto.FromLocationType, dto.FromLocationId, item.ProductId);

            if (inventory != null)
            {
                inventory.ReservedQuantity += item.RequestedQuantity;
                await _inventoryRepository.UpdateAsync(inventory);
                _logger.LogInformation(
                    "Reserved {Qty} units of product {ProductId} at {Type}:{LocationId} for transfer {TransferNumber}",
                    item.RequestedQuantity, item.ProductId, dto.FromLocationType, dto.FromLocationId, created.TransferNumber);
            }
            else
            {
                _logger.LogWarning(
                    "Inventory not found for product {ProductId} at {Type}:{LocationId} — reserved_quantity not updated",
                    item.ProductId, dto.FromLocationType, dto.FromLocationId);
            }
        }

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

        var transfer = await _transferRepository.GetByIdAsync(id);
        if (transfer == null)
        {
            return false;
        }

        await _transferRepository.DeleteAsync(id);
        return true;
    }

    public async Task<TransferDto> ReceiveTransferAsync(Guid transferId, ReceiveTransferDto dto, Guid receivedBy)
    {
        _logger.LogInformation("Receiving transfer {TransferId} by user {UserId}", transferId, receivedBy);
        //1. Load transfer
        var transfer = await _transferRepository.GetByIdAsync(transferId)
            ?? throw new KeyNotFoundException($"Transfer {transferId} not found");

        //if (transfer.Status == "COMPLETED" || transfer.Status == "CANCELLED")
        //    throw new InvalidOperationException($"Transfer is already {transfer.Status} and cannot be received");

        if (transfer.RestockRequestId.HasValue)
        {
            var restockRequest = await _restockRequestRepository.GetByIdAsync(transfer.RestockRequestId.Value);
            if (restockRequest != null && restockRequest.Status != "PROCESSING")
            {
                throw new InvalidOperationException($"Cannot receive transfer because associated Restock Request {restockRequest.Id} is not in PROCESSING status (current: {restockRequest.Status}).");
            }
        }
        if (transfer.Status != "IN_TRANSIT")
            throw new InvalidOperationException($"Transfer must be in IN_TRANSIT status to be received. Current status: {transfer.Status}");

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
            transferItem.ReceivedQuantity = receivedQuantity;  // Tự tính
            transferItem.DamagedQuantity = incoming.DamagedQuantity;
            transferItem.Notes = incoming.Notes ?? transferItem.Notes;

            // 3. Cộng hàng tốt vào inventory của kho đích (ToLocation)
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
                    // Cộng vào quantity, AvailableQuantity tự tính (computed = quantity - reserved)
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
            // 4. Chuyển batch gốc sang nơi nhận và cập nhật số lượng thực nhận nếu có BatchId
            if (transferItem.BatchId.HasValue)
            {
                var batch = await _productBatchRepository.GetByIdAsync(transferItem.BatchId.Value)
                    ?? throw new KeyNotFoundException($"ProductBatch {transferItem.BatchId.Value} not found");

                batch.Quantity = receivedQuantity;
                batch.WarehouseId = transfer.ToLocationId;
                await _productBatchRepository.UpdateAsync(batch);
                _logger.LogInformation(
                    "ProductBatch {BatchId} moved to location {LocationId} with quantity updated to {Quantity}",
                    batch.Id, transfer.ToLocationId, batch.Quantity);
            }

            // Lấy unitPrice từ ProductService
            var product = await _productServiceClient.GetProductByIdAsync(transferItem.ProductId);
            var unitPrice = product?.OriginalPrice ?? 0;

            // 5. Gom StockMovementItem, quantity = netReceived (đã trừ hàng hư)
            stockMovementItems.Add(new StockMovementItem
            {
                Id = Guid.NewGuid(),
                ProductId = transferItem.ProductId,
                BatchId = transferItem.BatchId,
                Quantity = receivedQuantity,
                UnitPrice = unitPrice
            });
        } // Kết thúc foreach

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

        // 6. Cập nhật Transfer sang COMPLETED
        transfer.Status = "COMPLETED";
        transfer.ReceivedBy = receivedBy;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.ActualDelivery = DateTime.UtcNow;
        await _transferRepository.UpdateAsync(transfer);

        // 7. Cập nhật RestockRequest sang COMPLETED nếu có liên kết
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

    /// <summary>
    /// Xuất kho (outbound): giảm quantity từ kho nguồn, tạo StockMovement OUTBOUND
    /// </summary>
    public async Task<bool> CreateOutboundStockMovementAsync(Guid transferId, Guid shippedBy)
    {
        _logger.LogInformation("Creating outbound stock movement for transfer {TransferId}", transferId);

        // 1. Load transfer
        var transfer = await _transferRepository.GetByIdAsync(transferId)
            ?? throw new KeyNotFoundException($"Transfer {transferId} not found");

        var transferItems = transfer.TransferItems;
        var stockMovementItems = new List<StockMovementItem>();

        // 2. Giảm quantity và reserved_quantity tại kho nguồn
        foreach (var item in transferItems)
        {
            var sourceInventory = await _inventoryRepository
                .GetByLocationAndProductAsync(transfer.FromLocationType, transfer.FromLocationId, item.ProductId);
            if (sourceInventory != null)
            {
                sourceInventory.Quantity -= item.RequestedQuantity;
                sourceInventory.ReservedQuantity -= item.RequestedQuantity;
                sourceInventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(sourceInventory);

                // Lấy unitPrice từ ProductService
                var product = await _productServiceClient.GetProductByIdAsync(item.ProductId);
                var unitPrice = product?.OriginalPrice ?? 0;

                // 3. Tạo StockMovementItem với UnitPrice từ product.originalPrice
                stockMovementItems.Add(new StockMovementItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    BatchId = item.BatchId,
                    Quantity = item.RequestedQuantity,
                    UnitPrice = unitPrice
                });
            }
        }

        // 4. Tạo StockMovement OUTBOUND
        var movementCount = await _stockMovementRepository.CountByDateAsync(DateTime.UtcNow);
        var stockMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = $"SM-{DateTime.UtcNow:yyyyMMdd}-{(movementCount + 1):D3}",
            MovementType = "OUTBOUND",
            LocationId = transfer.FromLocationId,
            LocationType = transfer.FromLocationType,
            MovementDate = transfer.TransferDate,
            TransferId = transfer.Id,
            RestockRequestId = transfer.RestockRequestId,
            ReceivedBy = shippedBy, // Người giao hàng (outbound)
            Status = "COMPLETED",
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            StockMovementItems = stockMovementItems,
        };
        await _stockMovementRepository.AddAsync(stockMovement);

        // 5. Cập nhật transfer status sang IN_TRANSIT
        transfer.Status = "IN_TRANSIT";
        transfer.ShippedBy = shippedBy;
        transfer.UpdatedAt = DateTime.UtcNow;
        await _transferRepository.UpdateAsync(transfer);

        _logger.LogInformation("Outbound stock movement created for transfer {TransferId}", transferId);
        return true;
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

    private static void ValidateCreateTransfer(CreateTransferDto dto)
    {
        var validLocationTypes = new[] { "WAREHOUSE", "STORE" };

        if (string.IsNullOrWhiteSpace(dto.FromLocationType))
            throw new InvalidOperationException("FromLocationType is required.");

        if (string.IsNullOrWhiteSpace(dto.ToLocationType))
            throw new InvalidOperationException("ToLocationType is required.");

        if (!validLocationTypes.Contains(dto.FromLocationType.Trim().ToUpperInvariant()))
            throw new InvalidOperationException("FromLocationType must be WAREHOUSE or STORE.");

        if (!validLocationTypes.Contains(dto.ToLocationType.Trim().ToUpperInvariant()))
            throw new InvalidOperationException("ToLocationType must be WAREHOUSE or STORE.");

        if (dto.FromLocationId == Guid.Empty)
            throw new InvalidOperationException("FromLocationId is required.");

        if (dto.ToLocationId == Guid.Empty)
            throw new InvalidOperationException("ToLocationId is required.");

        if (dto.FromLocationId == dto.ToLocationId && string.Equals(dto.FromLocationType, dto.ToLocationType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Source and destination locations must be different.");

        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException("At least one transfer item is required.");

        if (dto.Items.Any(item => item.ProductId == Guid.Empty))
            throw new InvalidOperationException("Each transfer item must have a valid ProductId.");

        if (dto.Items.Any(item => item.RequestedQuantity <= 0))
            throw new InvalidOperationException("Each transfer item must have RequestedQuantity greater than 0.");
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
