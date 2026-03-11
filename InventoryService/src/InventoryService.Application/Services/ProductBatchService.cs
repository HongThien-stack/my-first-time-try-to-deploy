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