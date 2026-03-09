using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class WarehouseSlotService : IWarehouseSlotService
{
    private readonly IWarehouseSlotRepository _slotRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<WarehouseSlotService> _logger;

    public WarehouseSlotService(
        IWarehouseSlotRepository slotRepository,
        IWarehouseRepository warehouseRepository,
        ILogger<WarehouseSlotService> logger)
    {
        _slotRepository = slotRepository;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task<WarehouseSlotDto?> GetSlotByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting slot by ID: {SlotId}", id);

        var slot = await _slotRepository.GetByIdAsync(id);
        return slot == null ? null : MapToDto(slot);
    }

    public async Task<WarehouseSlotDto> CreateSlotAsync(Guid warehouseId, CreateSlotRequestDto request)
    {
        _logger.LogInformation("Creating slot {SlotCode} in warehouse {WarehouseId}", request.SlotCode, warehouseId);

        // Validate warehouse exists
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
        if (warehouse == null)
            throw new InvalidOperationException("Warehouse not found");

        // Validate slot code is unique within the warehouse
        if (await _slotRepository.ExistsSlotCodeAsync(warehouseId, request.SlotCode))
            throw new InvalidOperationException($"Slot code '{request.SlotCode}' already exists in this warehouse");

        var validStatuses = new[] { "EMPTY", "OCCUPIED", "RESERVED", "MAINTENANCE" };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

        var slot = new WarehouseSlot
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            SlotCode = request.SlotCode,
            Zone = request.Zone,
            RowNumber = request.RowNumber,
            ColumnNumber = request.ColumnNumber,
            Status = request.Status
        };

        var created = await _slotRepository.AddAsync(slot);

        // Reload with navigation property
        created.Warehouse = warehouse;
        return MapToDto(created);
    }

    public async Task<WarehouseSlotDto?> UpdateSlotAsync(Guid id, UpdateSlotRequestDto request)
    {
        _logger.LogInformation("Updating slot {SlotId}", id);

        var slot = await _slotRepository.GetByIdAsync(id);
        if (slot == null)
            return null;

        // If SlotCode is changing, check uniqueness
        if (!string.IsNullOrWhiteSpace(request.SlotCode) && request.SlotCode != slot.SlotCode)
        {
            if (await _slotRepository.ExistsSlotCodeAsync(slot.WarehouseId, request.SlotCode, excludeId: id))
                throw new InvalidOperationException($"Slot code '{request.SlotCode}' already exists in this warehouse");
            slot.SlotCode = request.SlotCode;
        }

        if (request.Zone != null)
            slot.Zone = string.IsNullOrWhiteSpace(request.Zone) ? null : request.Zone;

        if (request.RowNumber.HasValue)
            slot.RowNumber = request.RowNumber;

        if (request.ColumnNumber.HasValue)
            slot.ColumnNumber = request.ColumnNumber;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var validStatuses = new[] { "EMPTY", "OCCUPIED", "RESERVED", "MAINTENANCE" };
            if (!validStatuses.Contains(request.Status))
                throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
            slot.Status = request.Status;
        }

        await _slotRepository.UpdateAsync(slot);
        return MapToDto(slot);
    }

    public async Task<bool> DeleteSlotAsync(Guid id)
    {
        _logger.LogInformation("Deleting slot {SlotId}", id);

        var slot = await _slotRepository.GetByIdAsync(id);
        if (slot == null)
            return false;

        // Prevent deletion if slot is currently in use
        if (slot.Status == "OCCUPIED")
            throw new InvalidOperationException("Cannot delete a slot that is currently OCCUPIED");

        await _slotRepository.DeleteAsync(id);
        return true;
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static WarehouseSlotDto MapToDto(WarehouseSlot slot) => new()
    {
        Id = slot.Id,
        WarehouseId = slot.WarehouseId,
        WarehouseName = slot.Warehouse?.Name ?? string.Empty,
        SlotCode = slot.SlotCode,
        Zone = slot.Zone,
        RowNumber = slot.RowNumber,
        ColumnNumber = slot.ColumnNumber,
        Status = slot.Status,
        CreatedAt = slot.CreatedAt
    };
}
