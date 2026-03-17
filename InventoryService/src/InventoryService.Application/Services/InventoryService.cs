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

    public async Task<Inventory?> GetInventoryByLocationIdAndProductIdAsync(Guid deliverWarehouseId, Guid productId)
    {
        return await _inventoryRepository.GetInventoryByLocationIdAndProductIdAsync(deliverWarehouseId, productId);
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

    public async Task UpdateReservedQuantityAsync(Inventory inventory)
    {
        await _inventoryRepository.UpdateReservedQuantityAsync(inventory);
    }

    public async Task<InventoryDto> CheckOrCreateInventoryAsync(CreateInventoryDto dto)
    {
        _logger.LogInformation("Checking or creating inventory for product {ProductId} at location {LocationType}:{LocationId}", 
            dto.ProductId, dto.LocationType, dto.LocationId);

        // Check if inventory already exists
        var existingInventory = await _inventoryRepository.GetInventoryByLocationIdAndProductIdAsync(dto.LocationId, dto.ProductId);
        
        if (existingInventory != null)
        {
            _logger.LogInformation("Inventory already exists for product {ProductId} at location {LocationId}", 
                dto.ProductId, dto.LocationId);
            return MapToDto(existingInventory);
        }

        // Create new inventory if it doesn't exist
        var newInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            LocationType = dto.LocationType,
            LocationId = dto.LocationId,
            Quantity = dto.Quantity,
            ReservedQuantity = 0,
            MinStockLevel = dto.MinStockLevel ?? 10,
            MaxStockLevel = dto.MaxStockLevel ?? 1000,
            LastStockCheck = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdInventory = await _inventoryRepository.AddAsync(newInventory);
        _logger.LogInformation("New inventory created with ID {InventoryId}", createdInventory.Id);

        return MapToDto(createdInventory);
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
