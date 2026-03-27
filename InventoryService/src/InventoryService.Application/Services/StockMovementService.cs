using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IStockMovementRepository _repository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(
        IStockMovementRepository repository,
        IProductServiceClient productServiceClient,
        ILogger<StockMovementService> logger)
    {
        _repository = repository;
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<StockMovementDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all stock movements");
        var movements = await _repository.GetAllAsync();
        return movements.Select(m => MapToDto(m));
    }

    public async Task<StockMovementDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving stock movement {MovementId}", id);
        var movement = await _repository.GetByIdAsync(id);
        if (movement == null) return null;

        // Batch-fetch product info for all items in this movement
        var productIds = movement.StockMovementItems.Select(i => i.ProductId);
        var productMap = await _productServiceClient.GetProductsByIdsAsync(productIds);

        return MapToDto(movement, productMap);
    }

    public async Task<IEnumerable<StockMovementDto>> GetByLocationIdAsync(Guid locationId)
    {
        _logger.LogInformation("Retrieving stock movements for location {LocationId}", locationId);
        var movements = await _repository.GetByLocationIdAsync(locationId);
        return movements.Select(m => MapToDto(m));
    }

    public async Task<IEnumerable<StockMovementItemDto>> GetItemsByMovementIdAsync(Guid movementId)
    {
        _logger.LogInformation("Retrieving items for stock movement {MovementId}", movementId);
        var movement = await _repository.GetByIdAsync(movementId);
        if (movement == null)
        {
            throw new KeyNotFoundException($"Stock movement {movementId} not found");
        }

        // Batch-fetch product info for all items
        var productIds = movement.StockMovementItems.Select(i => i.ProductId);
        var productMap = await _productServiceClient.GetProductsByIdsAsync(productIds);

        return movement.StockMovementItems.Select(i => MapItemToDto(i, productMap));
    }

    public async Task AddNewStockMovementAsync(StockMovement stockMovement)
    {
        await _repository.AddNewStockMovementAsync(stockMovement);
    }

    public async Task AddNewStockMovementItemAsync(StockMovementItem stockMovementItem)
    {
        await _repository.AddNewStockMovementItemAsync(stockMovementItem);
    }

    public async Task<int> CountStockMovementAsync()
    {
        return await _repository.CountStockMovementAsync();
    }

    // Used for list endpoints (no product enrichment needed)
    private static StockMovementDto MapToDto(StockMovement m) => MapToDto(m, null);

    // Used for detail endpoints — enriches items with ProductName + Unit
    private static StockMovementDto MapToDto(StockMovement m, Dictionary<Guid, ProductInfoDto> productMap)
    {
        return new StockMovementDto
        {
            Id = m.Id,
            MovementNumber = m.MovementNumber,
            MovementType = m.MovementType,
            LocationId = m.LocationId,
            LocationType = m.LocationType,
            MovementDate = m.MovementDate,
            SupplierName = m.SupplierName,
            TransferId = m.TransferId,
            ReceivedBy = m.ReceivedBy,
            Status = m.Status,
            Notes = m.Notes,
            CreatedAt = m.CreatedAt,
            TotalItems = m.StockMovementItems.Count,
            Items = m.StockMovementItems.Select(i => MapItemToDto(i, productMap))
        };
    }

    private static StockMovementItemDto MapItemToDto(StockMovementItem i,
        Dictionary<Guid, ProductInfoDto>? productMap)
    {
        ProductInfoDto? product = null;
        productMap?.TryGetValue(i.ProductId, out product);
        return new StockMovementItemDto
        {
            Id = i.Id,
            MovementId = i.MovementId,
            ProductId = i.ProductId,
            ProductName = product?.Name,
            Unit = product?.Unit,
            BatchId = i.BatchId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        };
    }
}