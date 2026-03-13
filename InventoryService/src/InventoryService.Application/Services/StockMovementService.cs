using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IStockMovementRepository _repository;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(
        IStockMovementRepository repository,
        ILogger<StockMovementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<StockMovementDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all stock movements");
        var movements = await _repository.GetAllAsync();
        return movements.Select(MapToDto);
    }

    public async Task<StockMovementDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving stock movement {MovementId}", id);
        var movement = await _repository.GetByIdAsync(id);
        return movement != null ? MapToDto(movement) : null;
    }

    public async Task<IEnumerable<StockMovementItemDto>> GetItemsByMovementIdAsync(Guid movementId)
    {
        _logger.LogInformation("Retrieving items for stock movement {MovementId}", movementId);
        var movement = await _repository.GetByIdAsync(movementId);
        if (movement == null)
        {
            throw new KeyNotFoundException($"Stock movement {movementId} not found");
        }
        return movement.StockMovementItems.Select(MapItemToDto);
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

    private static StockMovementDto MapToDto(StockMovement m)
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
            Items = m.StockMovementItems.Select(MapItemToDto)
        };
    }

    private static StockMovementItemDto MapItemToDto(StockMovementItem i)
    {
        return new StockMovementItemDto
        {
            Id = i.Id,
            MovementId = i.MovementId,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        };
    }
}