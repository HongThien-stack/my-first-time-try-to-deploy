using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IWarehouseRepository
{
    Task<IEnumerable<Warehouse>> GetAllAsync();
    Task<Warehouse?> GetByIdAsync(Guid id);
    Task AddWarehouseAsync(Warehouse warehouse);
    Task UpdateWarehouseAsync(Warehouse warehouse);
    Task DeleteWarehouseAsync(Guid id);
}
