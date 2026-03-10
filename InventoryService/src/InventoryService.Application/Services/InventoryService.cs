using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class InventoryManagementService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<InventoryManagementService> _logger;

    public InventoryManagementService(
        IInventoryRepository inventoryRepository,
        ILogger<InventoryManagementService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
    {
        _logger.LogInformation("Getting all inventories");
        
        var inventories = await _inventoryRepository.GetAllAsync();
        
        return inventories.Select(MapToDto);
    }

    public async Task<InventoryDto?> GetInventoryByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting inventory by ID: {InventoryId}", id);
        
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        
        return inventory != null ? MapToDto(inventory) : null;
    }

    public async Task<IEnumerable<InventoryDto>> GetInventoriesByLocationAsync(string locationType, Guid locationId)
    {
        _logger.LogInformation("Getting inventories by location: {LocationType}:{LocationId}", locationType, locationId);
        
        var inventories = await _inventoryRepository.GetByLocationAsync(locationType, locationId);
        
        return inventories.Select(MapToDto);
    }

    public async Task<IEnumerable<InventoryDto>> GetInventoriesByProductAsync(Guid productId)
    {
        _logger.LogInformation("Getting inventories by product: {ProductId}", productId);
        
        var inventories = await _inventoryRepository.GetByProductIdAsync(productId);
        
        return inventories.Select(MapToDto);
    }

    public async Task<IEnumerable<InventoryDto>> GetLowStockItemsAsync(string? locationType = null)
    {
        _logger.LogInformation("Getting low stock items");
        
        var inventories = await _inventoryRepository.GetLowStockItemsAsync(locationType);
        
        return inventories.Select(MapToDto);
    }

    public async Task<InventoryDto> UpdateInventoryAsync(Guid id, int quantity, Guid performedBy, string reason)
    {
        _logger.LogInformation("Updating inventory {InventoryId} to quantity {Quantity}", id, quantity);
        
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
        {
            throw new KeyNotFoundException($"Inventory {id} not found");
        }

        inventory.Quantity = quantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);
        
        return MapToDto(inventory);
    }

    private InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            LocationType = inventory.LocationType,
            LocationId = inventory.LocationId,
            Quantity = inventory.Quantity,
            ReservedQuantity = inventory.ReservedQuantity,
            AvailableQuantity = inventory.AvailableQuantity,
            MinStockLevel = inventory.MinStockLevel,
            MaxStockLevel = inventory.MaxStockLevel,
            LastStockCheck = inventory.LastStockCheck,
            UpdatedAt = inventory.UpdatedAt
        };
    }
}
