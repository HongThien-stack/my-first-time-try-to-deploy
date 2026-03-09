using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class TransferService : ITransferService
{
    private readonly ITransferRepository _transferRepository;
    public TransferService(ITransferRepository transferRepository)
    {
        _transferRepository = transferRepository;
    }

    public async Task AddNewTransferAsync(Transfer transfer)
    {
        await _transferRepository.AddNewTransferAsync(transfer);
    }

    public async Task AddNewTransferItemAsync(TransferItem transferItem)
    {
        await _transferRepository.AddNewTransferItemAsync(transferItem);
    }

    public async Task<List<Transfer>> GetAllTransfersAsync()
    {
        return await _transferRepository.GetAllTransfersAsync();
    }

    public async Task<List<TransferItem>> GetAllTransferItemsByIdAsync(Guid transferId)
    {
        return await _transferRepository.GetAllTransferItemsByIdAsync(transferId);
    }

    public async Task<Transfer?> GetTransferByIdAsync(Guid id)
    {
        return await _transferRepository.GetTransferByIdAsync(id);
    }

    public async Task<int> CountTransferAsync()
    {
        return await _transferRepository.CountTransferAsync();
    }
}
