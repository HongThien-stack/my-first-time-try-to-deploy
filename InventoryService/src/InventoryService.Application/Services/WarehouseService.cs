using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductBatchRepository _productBatchRepository;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        IWarehouseRepository warehouseRepository,
        IInventoryRepository inventoryRepository,
        IProductBatchRepository productBatchRepository,
        ILogger<WarehouseService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
        _productBatchRepository = productBatchRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
    {
        _logger.LogInformation("Getting all warehouses");
        
        var warehouses = await _warehouseRepository.GetAllAsync();
        
        return warehouses
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Name = w.Name,
                Location = w.Location,
                Capacity = w.Capacity,
                Status = w.Status,
                ParentId = w.ParentId,
                IsDeleted = w.IsDeleted,
                CreatedAt = w.CreatedAt,
                CreatedBy = w.CreatedBy
            });
    }

    public async Task<List<Warehouse>> GetAllWarehouseByParentIdAsync(Guid parentId)
    {
        _logger.LogInformation("Getting all warehouses by parent ID: {ParentId}", parentId);
        return await _warehouseRepository.GetAllWarehouseByParentIdAsync(parentId);
    }

    public async Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting warehouse by ID: {WarehouseId}", id);
        
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        
        if (warehouse == null)
        {
            return null;
        }

        return new WarehouseDto
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Location = warehouse.Location,
            Capacity = warehouse.Capacity,
            Status = warehouse.Status,
            ParentId = warehouse.ParentId,
            IsDeleted = warehouse.IsDeleted,
            CreatedAt = warehouse.CreatedAt,
            CreatedBy = warehouse.CreatedBy
        };
    }

    public async Task<Warehouse?> GetWarehouseAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        
        if (warehouse == null)
        {
            return null;
        }
        return warehouse;
    }

    public async Task AddWarehouseAsync(Warehouse warehouse)
    {
        await _warehouseRepository.AddWarehouseAsync(warehouse);
    }

    public async Task UpdateWarehouseAsync(Warehouse warehouse)
    {
        await _warehouseRepository.UpdateWarehouseAsync(warehouse);
    }

    /// <summary>
    /// Soft deletes a warehouse after validating that it is empty.
    /// </summary>
    /// <param name="id">The identifier of the warehouse to delete.</param>
    /// <remarks>
    /// The warehouse may only be deleted when:
    /// - all inventory records for the warehouse have Quantity == 0
    /// - all product batches in the warehouse have Quantity == 0
    /// </remarks>
    public async Task DeleteWarehouseAsync(Guid id)
    {
        // Verify the warehouse exists and is not already soft deleted.
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null || warehouse.IsDeleted)
        {
            throw new KeyNotFoundException($"Warehouse {id} not found");
        }

        // Load inventory entries for this warehouse.
        var warehouseInventories = (await _inventoryRepository.GetByLocationAsync("WAREHOUSE", id)).ToList();

        // Reject deletion if any inventory still has positive quantity.
        if (warehouseInventories.Any(i => i.Quantity > 0))
        {
            throw new InvalidOperationException("Cannot delete warehouse because some inventory records still have quantity > 0.");
        }

        // Load batches for this warehouse.
        var warehouseBatches = (await _productBatchRepository.GetByWarehouseIdAsync(id)).ToList();
        var batchWithQuantity = warehouseBatches.FirstOrDefault(b => b.Quantity > 0);

        // Reject deletion if any batch still holds positive quantity.
        if (batchWithQuantity != null)
        {
            throw new InvalidOperationException("Cannot delete warehouse because some product batches still have quantity > 0.");
        }

        // All validations passed: soft delete the warehouse.
        await _warehouseRepository.DeleteWarehouseAsync(id);
    }
}
