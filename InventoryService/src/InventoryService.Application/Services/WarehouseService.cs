using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        IWarehouseRepository warehouseRepository,
        ILogger<WarehouseService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
    {
        _logger.LogInformation("Getting all warehouses");
        
        var warehouses = await _warehouseRepository.GetAllAsync();
        
        return warehouses
            .Where(w => !w.IsDeleted)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Name = w.Name,
                Location = w.Location,
                Capacity = w.Capacity,
                Status = w.Status,
                IsDeleted = w.IsDeleted,
                CreatedAt = w.CreatedAt
            });
    }

    public async Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting warehouse by ID: {WarehouseId}", id);
        
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        
        if (warehouse == null || warehouse.IsDeleted)
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
            IsDeleted = warehouse.IsDeleted,
            CreatedAt = warehouse.CreatedAt
        };
    }

    public async Task<Warehouse?> GetWarehouseAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        
        if (warehouse == null || warehouse.IsDeleted)
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

    public async Task DeleteWarehouseAsync(Guid id) {
        await _warehouseRepository.DeleteWarehouseAsync(id);
    }
}
