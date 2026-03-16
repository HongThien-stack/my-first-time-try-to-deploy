using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class ProductBatchService : IProductBatchService
{
    private readonly IProductBatchRepository _repository;
    private readonly ILogger<ProductBatchService> _logger;

    public ProductBatchService(
        IProductBatchRepository repository,
        ILogger<ProductBatchService> logger)
    {
        _repository = repository;
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