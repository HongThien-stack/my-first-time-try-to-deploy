using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class TransferService : ITransferService
{
    private readonly ITransferRepository _transferRepository;
    private readonly ILogger<TransferService> _logger;

    public TransferService(
        ITransferRepository transferRepository,
        ILogger<TransferService> logger)
    {
        _transferRepository = transferRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TransferDto>> GetAllTransfersAsync()
    {
        _logger.LogInformation("Getting all transfers");
        var transfers = await _transferRepository.GetAllAsync();
        return transfers.Select(MapToDto);
    }

    public async Task<TransferDto?> GetTransferByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting transfer by ID: {TransferId}", id);
        var transfer = await _transferRepository.GetByIdAsync(id);
        return transfer != null ? MapToDto(transfer) : null;
    }

    public async Task<TransferDto?> GetTransferByNumberAsync(string transferNumber)
    {
        _logger.LogInformation("Getting transfer by number: {TransferNumber}", transferNumber);
        var transfer = await _transferRepository.GetByTransferNumberAsync(transferNumber);
        return transfer != null ? MapToDto(transfer) : null;
    }

    public async Task<IEnumerable<TransferDto>> GetTransfersByStatusAsync(string status)
    {
        _logger.LogInformation("Getting transfers by status: {Status}", status);
        var transfers = await _transferRepository.GetByStatusAsync(status);
        return transfers.Select(MapToDto);
    }

    public async Task<TransferDto> CreateTransferAsync(CreateTransferDto dto)
    {
        _logger.LogInformation("Creating new transfer from {FromType}:{FromId} to {ToType}:{ToId}",
            dto.FromLocationType, dto.FromLocationId, dto.ToLocationType, dto.ToLocationId);

        var transferNumber = await GenerateTransferNumberAsync();
        
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            TransferNumber = transferNumber,
            FromLocationType = dto.FromLocationType,
            FromLocationId = dto.FromLocationId,
            ToLocationType = dto.ToLocationType,
            ToLocationId = dto.ToLocationId,
            TransferDate = DateTime.UtcNow,
            ExpectedDelivery = dto.ExpectedDelivery,
            Status = "PENDING",
            ShippedBy = dto.ShippedBy,
            Notes = dto.Notes,
            TransferItems = dto.Items.Select(item => new TransferItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                BatchId = item.BatchId,
                RequestedQuantity = item.RequestedQuantity
            }).ToList()
        };

        var created = await _transferRepository.AddAsync(transfer);
        return MapToDto(created);
    }

    public async Task UpdateTransferStatusAsync(Guid id, string status, Guid? userId = null)
    {
        _logger.LogInformation("Updating transfer {TransferId} status to {Status}", id, status);
        
        var transfer = await _transferRepository.GetByIdAsync(id);
        if (transfer == null)
        {
            throw new KeyNotFoundException($"Transfer {id} not found");
        }

        transfer.Status = status;
        
        if (status == "IN_TRANSIT" && userId.HasValue)
        {
            transfer.ShippedBy = userId.Value;
        }
        else if (status == "DELIVERED" && userId.HasValue)
        {
            transfer.ReceivedBy = userId.Value;
            transfer.ActualDelivery = DateTime.UtcNow;
        }

        await _transferRepository.UpdateAsync(transfer);
    }

    public async Task<bool> DeleteTransferAsync(Guid id)
    {
        _logger.LogInformation("Deleting transfer: {TransferId}", id);
        await _transferRepository.DeleteAsync(id);
        return true;
    }

    private TransferDto MapToDto(Transfer transfer)
    {
        return new TransferDto
        {
            Id = transfer.Id,
            TransferNumber = transfer.TransferNumber,
            FromLocationType = transfer.FromLocationType,
            FromLocationId = transfer.FromLocationId,
            ToLocationType = transfer.ToLocationType,
            ToLocationId = transfer.ToLocationId,
            TransferDate = transfer.TransferDate,
            ExpectedDelivery = transfer.ExpectedDelivery,
            ActualDelivery = transfer.ActualDelivery,
            Status = transfer.Status,
            ShippedBy = transfer.ShippedBy,
            ReceivedBy = transfer.ReceivedBy,
            Notes = transfer.Notes,
            Items = transfer.TransferItems.Select(item => new TransferItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                BatchId = item.BatchId,
                RequestedQuantity = item.RequestedQuantity,
                ShippedQuantity = item.ShippedQuantity,
                ReceivedQuantity = item.ReceivedQuantity,
                DamagedQuantity = item.DamagedQuantity,
                Notes = item.Notes
            }).ToList()
        };
    }

    private async Task<string> GenerateTransferNumberAsync()
    {
        var now = DateTime.UtcNow;
        var prefix = $"TRF-{now:yyyy}-";
        var allTransfers = await _transferRepository.GetAllAsync();
        var todayTransfers = allTransfers.Where(t => t.TransferNumber.StartsWith(prefix)).Count();
        return $"{prefix}{(todayTransfers + 1):D3}";
    }
}
