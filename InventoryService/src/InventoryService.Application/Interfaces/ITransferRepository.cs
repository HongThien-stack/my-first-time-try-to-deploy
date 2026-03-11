using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface ITransferRepository
{
    Task<IEnumerable<Transfer>> GetAllAsync();
    Task<Transfer?> GetByIdAsync(Guid id);
    Task<Transfer?> GetByTransferNumberAsync(string transferNumber);
    Task<IEnumerable<Transfer>> GetByStatusAsync(string status);
    Task<Transfer> AddAsync(Transfer transfer);
    Task UpdateAsync(Transfer transfer);
    Task DeleteAsync(Guid id);
}
