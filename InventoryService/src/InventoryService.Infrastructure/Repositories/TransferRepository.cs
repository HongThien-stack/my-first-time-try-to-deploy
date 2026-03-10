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

    public async Task AddNewTransferAsync(Transfer transfer)
    {
        await _context.Transfers.AddAsync(transfer);
        await _context.SaveChangesAsync();
    }

    public async Task AddNewTransferItemAsync(TransferItem transferItem)
    {
        await _context.TransferItems.AddAsync(transferItem);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Transfer>> GetAllTransfersAsync()
    {
        return await _context.Transfers.ToListAsync();

    }

    public async Task<List<TransferItem>> GetAllTransferItemsByIdAsync(Guid transferId)
    {
        return await _context.TransferItems
                    .Where(ti => ti.TransferId == transferId)
                    .ToListAsync();
    }

    public async Task<Transfer?> GetTransferByIdAsync(Guid id)
    {
        return await _context.Transfers
                    .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<int> CountTransferAsync()
    {
        return await _context.Transfers.CountAsync();
    }

    // Cap nhat thong tin transfer 
    public async Task UpdateTransferAsync(Transfer transfer)
    {
        _context.Transfers.Update(transfer);
        await _context.SaveChangesAsync();
    }
}
