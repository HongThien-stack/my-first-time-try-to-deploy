using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class ProductBatchService : IProductBatchService
{
    private readonly IProductBatchRepository _repository;
    private readonly IRestockRequestRepository _restockRequestRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ILogger<ProductBatchService> _logger;

    public ProductBatchService(
        IProductBatchRepository repository,
        IRestockRequestRepository restockRequestRepository,
        IInventoryRepository inventoryRepository,
        IStockMovementRepository stockMovementRepository,
        ILogger<ProductBatchService> logger)
    {
        _repository = repository;
        _stockMovementRepository = stockMovementRepository;
        _restockRequestRepository = restockRequestRepository;
        _inventoryRepository = inventoryRepository;
        _logger = logger;
        
    }

    public async Task<IEnumerable<ProductBatchDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all product batches");
        var batches = await _repository.GetAllAsync();
        return batches.Select(MapToDto);
    }

    public async Task<ProductBatchDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving product batch {BatchId}", id);
        var batch = await _repository.GetByIdAsync(id);
        return batch != null ? MapToDto(batch) : null;
    }

    public async Task<IEnumerable<ProductBatchDto>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        _logger.LogInformation("Retrieving product batches for warehouse {WarehouseId}", warehouseId);
        var batches = await _repository.GetByWarehouseIdAsync(warehouseId);
        
        // Sort by expiry date: products about to expire first (ascending), then products with long shelf life
        var sortedBatches = batches
            .OrderBy(b => b.ExpiryDate == null ? DateTime.MaxValue : b.ExpiryDate) // Null expiry dates go to bottom
            .Select(MapToDto);
        
        return sortedBatches;
    }

    /// <summary>
    /// Allocate/split a product batch: create a new batch from an existing batch
    /// with the specified quantity, reduce the source batch by that amount
    /// </summary>
    public async Task<ProductBatchDto> AllocateBatchAsync(CreateAllocatedBatchDto dto)
    {
        _logger.LogInformation("Allocating {Qty} units from batch {SourceBatchId}", dto.AllocatedQuantity, dto.SourceBatchId);

        // Validate source batch exists and has sufficient quantity
        var sourceBatch = await _repository.GetByIdAsync(dto.SourceBatchId)
            ?? throw new KeyNotFoundException($"Source batch {dto.SourceBatchId} not found");

        if (sourceBatch.Quantity < dto.AllocatedQuantity)
            throw new InvalidOperationException($"Insufficient quantity. Available: {sourceBatch.Quantity}, Requested: {dto.AllocatedQuantity}");

        // Create new batch (allocated batch)
        var newBatch = new ProductBatch
        {
            Id = Guid.NewGuid(),
            ProductId = sourceBatch.ProductId,
            WarehouseId = dto.TargetWarehouseId ?? sourceBatch.WarehouseId,
            BatchNumber = $"{sourceBatch.BatchNumber}-ALLOC-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Quantity = dto.AllocatedQuantity,
            ManufacturingDate = sourceBatch.ManufacturingDate,
            ExpiryDate = sourceBatch.ExpiryDate,
            Supplier = sourceBatch.Supplier,
            SupplierId = sourceBatch.SupplierId,
            ReceivedAt = DateTime.UtcNow,
            Status = sourceBatch.Status
        };

        // Add new batch
        var createdBatch = await _repository.AddAsync(newBatch);

        // Reduce source batch quantity
        sourceBatch.Quantity -= dto.AllocatedQuantity;
        await _repository.UpdateAsync(sourceBatch);

        _logger.LogInformation("Batch {SourceBatchId} reduced by {Qty}. New batch {NewBatchId} created with {AllocatedQty}",
            sourceBatch.Id, dto.AllocatedQuantity, createdBatch.Id, dto.AllocatedQuantity);

        return MapToDto(createdBatch);
    }


    public async Task<IEnumerable<ProductBatchDto>> ReceiveFromSupplierAsync(ReceiveFromSupplierDto dto)
    {
        _logger.LogInformation("Receiving goods from supplier for RestockRequest {RestockRequestId}", dto.RestockRequestId);
        //1. Validate RstockRequest exists
        var restockRequest = await _restockRequestRepository.GetByIdAsync(dto.RestockRequestId)
            ?? throw new KeyNotFoundException($"RestockRequest {dto.RestockRequestId} not found");
        
        if (restockRequest.Status != "APPROVED")
        {
            throw new InvalidOperationException(
                restockRequest.Status is "COMPLETED" or "CANCELLED"
                    ? $"RestockRequest is already in '{restockRequest.Status}' state."
                    : $"RestockRequest {dto.RestockRequestId} is not approved. Current status: {restockRequest.Status}");
        }
        var createdBatches = new List<ProductBatch>();
        var stockMovementItems = new List<StockMovementItem>();

        // 2.Create ProductBatch cho mỗi item.
        foreach (var item in dto.Items)
        {
            // Tự động tạo BatchNumber nếu không được cung cấp
            //var batchNumber = !string.IsNullOrWhiteSpace(item.BatchNumber)
            //    ? item.BatchNumber
            //    : $"BN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var newBatch = new ProductBatch
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                WarehouseId = dto.WarehouseId,
                BatchNumber = item.BatchNumber,
                Quantity = item.Quantity,
                ManufacturingDate = item.ManufacturingDate,
                ExpiryDate = item.ExpiryDate,
                Supplier = item.SupplierName,
                SupplierId = item.SupplierId,
                ReceivedAt = DateTime.UtcNow,
                Status = "AVAILABLE"
            };
            // tạo batch và lưu vào database để lấy Id cho StockMovementItem
            var createdBatch = await _repository.AddAsync(newBatch);
            createdBatches.Add(createdBatch);

            // tạo StockMovementItem cho mỗi batch được tạo ra
            stockMovementItems.Add(new StockMovementItem
            {
                Id = Guid.NewGuid(),
                ProductId = createdBatch.ProductId,
                BatchId = createdBatch.Id,
                Quantity = createdBatch.Quantity,
                UnitPrice = item.UnitPrice
            });

            // 3. Update Inventory: tăng quantity lên
            var inventory = await _inventoryRepository.GetByLocationAndProductAsync("WAREHOUSE", dto.WarehouseId, item.ProductId);
            if (inventory != null)
            {
                inventory.Quantity += item.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);
            }
            else
            {
                await _inventoryRepository.AddAsync(new Inventory
                // Nếu chưa có record Inventory nào cho product này tại warehouse này, tạo mới
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    LocationType = "WAREHOUSE",
                    LocationId = dto.WarehouseId,
                    Quantity = item.Quantity,
                    UpdatedAt = DateTime.UtcNow
                });
                
            }
            _logger.LogInformation("Inventory for Product {ProductId} at Warehouse {WarehouseId} updated by {Quantity}", item.ProductId, dto.WarehouseId, item.Quantity);
        }

        // 4. Tạo StockMovement Nhập kho
        var movementCount = await _stockMovementRepository.CountByDateAsync(DateTime.UtcNow);
        var stockMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = $"SM-{DateTime.UtcNow:yyyyMMdd}-{(movementCount + 1):D3}",
            MovementType = "INBOUND",
            LocationId = dto.WarehouseId,
            LocationType = "WAREHOUSE",
            MovementDate = DateTime.UtcNow,
            RestockRequestId = dto.RestockRequestId,
            ReceivedBy = dto.ReceivedBy,
            TransferId = null,
            Status = "COMPLETED",
            Notes = dto.Notes,
            StockMovementItems = stockMovementItems
        };
        await _stockMovementRepository.AddAsync(stockMovement);
        _logger.LogInformation("Created INBOUND StockMovement {MovementNumber}", stockMovement.MovementNumber);

        //5. Cập nhật status của Restock Request
        restockRequest.Status = "COMPLETED";
        restockRequest.UpdatedAt = DateTime.UtcNow;
        await _restockRequestRepository.UpdateAsync(restockRequest);
        _logger.LogInformation("RestockRequest {RestockRequestId} status updated to COMPLETED", dto.RestockRequestId);

        return createdBatches.Select(MapToDto);

    }




    /// <summary>
    /// Automatically update batch status to EXPIRED based on expiry date
    /// Called by background service at scheduled intervals
    /// </summary>
    public async Task<int> UpdateExpiredBatchesAsync()
    {
        _logger.LogInformation("Starting automatic expired batch update");
        
        var allBatches = await _repository.GetAllAsync();
        var now = DateTime.UtcNow;
        var batchesToUpdate = allBatches.Where(b => 
            b.Status == "AVAILABLE" && 
            b.ExpiryDate.HasValue && 
            b.ExpiryDate.Value <= now).ToList();

        int updatedCount = 0;

        foreach (var batch in batchesToUpdate)
        {
            batch.Status = "EXPIRED";
            await _repository.UpdateAsync(batch);
            updatedCount++;
            _logger.LogInformation("Batch {BatchId} ({BatchNumber}) auto-marked as EXPIRED", batch.Id, batch.BatchNumber);
        }

        _logger.LogInformation("Completed auto-update of expired batches: {Count} batches updated", updatedCount);
        return updatedCount;
    }

    /// <summary>
    /// Create outbound stock movement for all expired batches at a warehouse
    /// </summary>
    public async Task<OutboundStockMovementResponseDto> CreateOutboundFromExpiredBatchesAsync(CreateOutboundFromExpiredBatchesDto request)
    {
        _logger.LogInformation("Creating outbound stock movement for expired batches at warehouse {WarehouseId}", request.WarehouseId);

        // Get all EXPIRED batches at the warehouse
        var warehouseBatches = await _repository.GetByWarehouseIdAsync(request.WarehouseId);
        var expiredBatches = warehouseBatches.Where(b => b.Status == "EXPIRED" && b.Quantity > 0).ToList();

        if (!expiredBatches.Any())
        {
            throw new InvalidOperationException($"No expired batches found at warehouse {request.WarehouseId}");
        }

        _logger.LogInformation("Found {Count} expired batches at warehouse {WarehouseId}", expiredBatches.Count, request.WarehouseId);

        // Create stock movement items
        var stockMovementItems = new List<StockMovementItem>();
        var processedBatches = new List<ExpiredBatchDetailDto>();
        int totalQuantity = 0;

        foreach (var batch in expiredBatches)
        {
            stockMovementItems.Add(new StockMovementItem
            {
                Id = Guid.NewGuid(),
                ProductId = batch.ProductId,
                BatchId = batch.Id,
                Quantity = batch.Quantity,
                UnitPrice = null
            });

            processedBatches.Add(new ExpiredBatchDetailDto
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                ProductId = batch.ProductId,
                Quantity = batch.Quantity,
                ExpiryDate = batch.ExpiryDate
            });

            totalQuantity += batch.Quantity;
        }

        // Create stock movement
        var movementCount = await _stockMovementRepository.CountStockMovementAsync();
        var movementNumber = $"SM-{DateTime.UtcNow:yyyyMMdd}-{(movementCount + 1):D3}";

        var stockMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = movementNumber,
            MovementType = "OUTBOUND",
            LocationId = request.WarehouseId,
            LocationType = "WAREHOUSE",
            MovementDate = DateTime.UtcNow,
            Status = "COMPLETED",
            Notes = $"Automatic outbound for expired batches. {request.Notes}",
            StockMovementItems = stockMovementItems
        };

        await _stockMovementRepository.AddAsync(stockMovement);
        _logger.LogInformation("Created OUTBOUND StockMovement {MovementNumber} for {Count} expired batches", movementNumber, expiredBatches.Count);

        // Update inventory for each expired batch
        foreach (var batch in expiredBatches)
        {
            var inventory = await _inventoryRepository.GetByLocationAndProductAsync("WAREHOUSE", request.WarehouseId, batch.ProductId);
            if (inventory != null)
            {
                inventory.Quantity -= batch.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);
                _logger.LogInformation("Inventory updated: Product {ProductId} at Warehouse {WarehouseId} reduced by {Quantity}", 
                    batch.ProductId, request.WarehouseId, batch.Quantity);
            }
        }

        return new OutboundStockMovementResponseDto
        {
            StockMovementId = stockMovement.Id,
            MovementNumber = stockMovement.MovementNumber,
            TotalBatchesProcessed = expiredBatches.Count,
            TotalQuantityOutbound = totalQuantity,
            MovementDate = stockMovement.MovementDate,
            Message = $"Outbound created for {expiredBatches.Count} expired batches with total quantity {totalQuantity}",
            ProcessedBatches = processedBatches
        };
    }

    private static ProductBatchDto MapToDto(ProductBatch b)
    {
        return new ProductBatchDto
        {
            Id = b.Id,
            ProductId = b.ProductId,
            WarehouseId = b.WarehouseId,
            BatchNumber = b.BatchNumber,
            Quantity = b.Quantity,
            ManufacturingDate = b.ManufacturingDate,
            ExpiryDate = b.ExpiryDate,
            Supplier = b.Supplier,
            ReceivedAt = b.ReceivedAt,
            Status = b.Status
        };
    }
}