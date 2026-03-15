using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly InventoryDbContext _context;
    public TransferRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transfer>> GetAllAsync()
    {
        return await _context.Transfers
            .Include(t => t.TransferItems)
            .ToListAsync();
    }

    public async Task<Transfer?> GetByIdAsync(Guid id)
    {
        return await _context.Transfers
            .Include(t => t.TransferItems)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transfer?> GetByTransferIdWithoutTransferItemAsync(Guid transferId)
    {
        return await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == transferId);
    }

    public async Task<Transfer?> GetByTransferNumberAsync(string transferNumber)
    {
        return await _context.Transfers
            .Include(t => t.TransferItems)
            .FirstOrDefaultAsync(t => t.TransferNumber == transferNumber);
    }

    public async Task<IEnumerable<Transfer>> GetByStatusAsync(string status)
    {
        return await _context.Transfers
            .Include(t => t.TransferItems)
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task<Transfer> AddAsync(Transfer transfer)
    {
        await _context.Transfers.AddAsync(transfer);
        await _context.SaveChangesAsync();
        return transfer;
    }

    public async Task UpdateAsync(Transfer transfer)
    {
        _context.Transfers.Update(transfer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var transfer = await _context.Transfers.FindAsync(id);
        if (transfer != null)
        {
            _context.Transfers.Remove(transfer);
            await _context.SaveChangesAsync();
        }
    }
}
