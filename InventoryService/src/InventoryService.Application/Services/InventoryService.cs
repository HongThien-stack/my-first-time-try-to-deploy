using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
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
        
        return inventories.Select(i => new InventoryDto
        {
            Id = i.Id,
            StoreId = i.StoreId,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            AlertThreshold = i.AlertThreshold,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt
        });
    }

    public async Task<InventoryDto?> GetInventoryByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting inventory by ID: {InventoryId}", id);
        
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        
        if (inventory == null)
        {
            return null;
        }

        return new InventoryDto
        {
            Id = inventory.Id,
            StoreId = inventory.StoreId,
            ProductId = inventory.ProductId,
            Quantity = inventory.Quantity,
            AlertThreshold = inventory.AlertThreshold,
            CreatedAt = inventory.CreatedAt,
            UpdatedAt = inventory.UpdatedAt
        };
    }

    public async Task<IEnumerable<InventoryDto>> GetInventoriesByStoreAsync(Guid storeId)
    {
        _logger.LogInformation("Getting inventories by store: {StoreId}", storeId);
        
        var inventories = await _inventoryRepository.GetByStoreIdAsync(storeId);
        
        return inventories.Select(i => new InventoryDto
        {
            Id = i.Id,
            StoreId = i.StoreId,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            AlertThreshold = i.AlertThreshold,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt
        });
    }

    public async Task<IEnumerable<InventoryDto>> GetInventoriesByProductAsync(Guid productId)
    {
        _logger.LogInformation("Getting inventories by product: {ProductId}", productId);
        
        var inventories = await _inventoryRepository.GetByProductIdAsync(productId);
        
        return inventories.Select(i => new InventoryDto
        {
            Id = i.Id,
            StoreId = i.StoreId,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            AlertThreshold = i.AlertThreshold,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt
        });
    }
}
