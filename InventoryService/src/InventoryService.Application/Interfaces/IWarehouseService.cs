using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IWarehouseService
{
    Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
    Task<List<Warehouse>> GetAllWarehouseByParentIdAsync(Guid parentId);
    Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id);
    Task<Warehouse?> GetWarehouseAsync(Guid id);
    Task AddWarehouseAsync(Warehouse warehouse);
    Task UpdateWarehouseAsync(Warehouse warehouse);
    Task DeleteWarehouseAsync(Guid id);
}
