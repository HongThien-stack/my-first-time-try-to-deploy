using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Services;

public class InventoryCheckService : IInventoryCheckService
{
    private readonly IInventoryCheckRepository _inventoryCheckRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryLockingRepository _inventoryLockingRepository;
    private readonly IInventoryLogRepository _inventoryLogRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<InventoryCheckService> _logger;

    public InventoryCheckService(
        IInventoryCheckRepository inventoryCheckRepository,
        IInventoryRepository inventoryRepository,
        IInventoryLockingRepository inventoryLockingRepository,
        IInventoryLogRepository inventoryLogRepository,
        IStockMovementRepository stockMovementRepository,
        IWarehouseRepository warehouseRepository,
        ILogger<InventoryCheckService> logger)
    {
        _inventoryCheckRepository = inventoryCheckRepository;
        _inventoryRepository = inventoryRepository;
        _inventoryLockingRepository = inventoryLockingRepository;
        _inventoryLogRepository = inventoryLogRepository;
        _stockMovementRepository = stockMovementRepository;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<InventoryCheckListDto>> GetAllInventoryChecksAsync(int? year = null, int? month = null)
    {
        try
        {
            var checks = await _inventoryCheckRepository.GetAllAsync(year, month);
            
            return checks.Select(c => new InventoryCheckListDto
            {
                Id = c.Id,
                CheckNumber = c.CheckNumber,
                LocationType = c.LocationType,
                LocationId = c.LocationId,
                CheckType = c.CheckType,
                CheckDate = c.CheckDate,
                CheckedBy = c.CheckedBy,
                Status = c.Status,
                TotalDiscrepancies = c.TotalDiscrepancies,
                CreatedAt = c.CreatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all inventory checks");
            throw;
        }
    }

    public async Task<InventoryCheckDto?> GetInventoryCheckByIdAsync(Guid id)
    {
        try
        {
            var check = await _inventoryCheckRepository.GetByIdAsync(id);
            
            if (check == null)
            {
                return null;
            }

            return new InventoryCheckDto
            {
                Id = check.Id,
                CheckNumber = check.CheckNumber,
                LocationType = check.LocationType,
                LocationId = check.LocationId,
                CheckType = check.CheckType,
                CheckDate = check.CheckDate,
                CheckedBy = check.CheckedBy,
                Status = check.Status,
                TotalDiscrepancies = check.TotalDiscrepancies,
                Notes = check.Notes,
                CreatedAt = check.CreatedAt,
                Items = check.InventoryCheckItems.Select(item => new InventoryCheckItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    SystemQuantity = item.SystemQuantity,
                    ActualQuantity = item.ActualQuantity,
                    Difference = item.Difference,
                    Note = item.Note
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory check by id: {Id}", id);
            throw;
        }
    }

    // =====================================================
    // Step 1: Create Inventory Check Session
    // BACKEND RULE: STAFF role required
    // BACKEND RULE: Only one PENDING check per location allowed
    // =====================================================
    public async Task<InventoryCheckDto> CreateInventoryCheckAsync(CreateInventoryCheckDto dto, UserContext userContext)
    {
        _logger.LogInformation("User {UserId} (Role: {Role}) creating inventory check for {LocationType}:{LocationId}", 
            userContext.UserId, userContext.Role, dto.LocationType, dto.LocationId);

        // BACKEND RULE 1: Validate location
        await ValidateLocationAsync(dto.LocationType, dto.LocationId);

        // BACKEND RULE 2: Validate check type
        if (!IsValidCheckType(dto.CheckType))
        {
            throw new ArgumentException($"{ErrorCodes.InvalidCheckType}: Invalid check type: {dto.CheckType}. Must be FULL, PARTIAL, or SPOT.");
        }

        // BACKEND RULE 3: Validate location type
        if (!IsValidLocationType(dto.LocationType))
        {
            throw new ArgumentException($"{ErrorCodes.InvalidLocationType}: Invalid location type: {dto.LocationType}. Must be WAREHOUSE or STORE.");
        }

        // BACKEND RULE 4: Unique session rule - Check for active PENDING check at this location
        var existingChecks = await _inventoryCheckRepository.GetByLocationAsync(dto.LocationType, dto.LocationId);
        var activePendingCheck = existingChecks.FirstOrDefault(c => c.Status == "PENDING");
        
        if (activePendingCheck != null)
        {
            _logger.LogWarning("Active PENDING inventory check {CheckNumber} already exists for {LocationType}:{LocationId}",
                activePendingCheck.CheckNumber, dto.LocationType, dto.LocationId);
            throw new InvalidOperationException(
                $"{ErrorCodes.ActiveSessionExists}: An active inventory check ({activePendingCheck.CheckNumber}) already exists for this location. Please complete or cancel it first.");
        }

        // Generate check number
        var checkNumber = await GenerateCheckNumberAsync();

        var inventoryCheck = new InventoryCheck
        {
            Id = Guid.NewGuid(),
            CheckNumber = checkNumber,
            LocationType = dto.LocationType,
            LocationId = dto.LocationId,
            CheckType = dto.CheckType,
            CheckDate = DateTime.UtcNow,
            CheckedBy = userContext.UserId, // From authenticated context
            Status = "PENDING",
            TotalDiscrepancies = 0,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _inventoryCheckRepository.AddAsync(inventoryCheck);

        _logger.LogInformation("Created inventory check {CheckNumber} with ID {Id} by user {UserId}", 
            created.CheckNumber, created.Id, userContext.UserId);

        return new InventoryCheckDto
        {
            Id = created.Id,
            CheckNumber = created.CheckNumber,
            LocationType = created.LocationType,
            LocationId = created.LocationId,
            CheckType = created.CheckType,
            CheckDate = created.CheckDate,
            CheckedBy = created.CheckedBy,
            Status = created.Status,
            TotalDiscrepancies = created.TotalDiscrepancies,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt,
            Items = new List<InventoryCheckItemDto>()
        };
    }

    // =====================================================
    // Step 2: Submit Inventory Check Results
    // BACKEND RULE: STAFF role required
    // BACKEND RULE: Must be in PENDING status (state machine)
    // BACKEND RULE: No duplicate submissions
    // BACKEND RULE: Validate actual_quantity >= 0
    // =====================================================
    public async Task<InventoryCheckDto> SubmitInventoryCheckAsync(Guid id, SubmitInventoryCheckDto dto, UserContext userContext)
    {
        _logger.LogInformation("User {UserId} (Role: {Role}) submitting inventory check {Id}",
            userContext.UserId, userContext.Role, id);

        var check = await _inventoryCheckRepository.GetByIdAsync(id);
        if (check == null)
        {
            throw new KeyNotFoundException($"{ErrorCodes.InventoryCheckNotFound}: Inventory check {id} not found");
        }

        // BACKEND RULE 1: State machine - Must be PENDING to submit
        if (check.Status != "PENDING")
        {
            _logger.LogWarning("Invalid state transition: Cannot submit check {CheckNumber} with status {Status}",
                check.CheckNumber, check.Status);
            throw new InvalidOperationException(
                $"{ErrorCodes.InvalidStateTransition}: Inventory check {id} cannot be submitted. Current status: {check.Status}. Expected: PENDING");
        }

        // BACKEND RULE 2: Prevent duplicate submission
        if (check.InventoryCheckItems.Any())
        {
            _logger.LogWarning("Duplicate submission attempt for check {CheckNumber}",check.CheckNumber);
            throw new InvalidOperationException(
                $"{ErrorCodes.InventoryCheckAlreadySubmitted}: Inventory check {id} has already been submitted");
        }

        // BACKEND RULE 3: Validate negative quantities
        foreach (var item in dto.Items)
        {
            if (item.ActualQuantity < 0)
            {
                throw new ArgumentException(
                    $"{ErrorCodes.NegativeQuantity}: Actual quantity cannot be negative for product {item.ProductId}");
            }
        }

        var checkItems = new List<InventoryCheckItem>();
        int totalDiscrepancies = 0;

        foreach (var item in dto.Items)
        {
            // BACKEND RULE 4: Get system quantity from inventories
            var inventory = await _inventoryRepository.GetByLocationAndProductAsync(
                check.LocationType, check.LocationId, item.ProductId);

            if (inventory == null)
            {
                _logger.LogWarning("Product {ProductId} not found in inventory for {LocationType}:{LocationId}", 
                    item.ProductId, check.LocationType, check.LocationId);
                throw new KeyNotFoundException(
                    $"{ErrorCodes.ProductNotInInventory}: Product {item.ProductId} not found in inventory for {check.LocationType} {check.LocationId}");
            }

            var checkItem = new InventoryCheckItem
            {
                Id = Guid.NewGuid(),
                CheckId = check.Id,
                ProductId = item.ProductId,
                SystemQuantity = inventory.Quantity,
                ActualQuantity = item.ActualQuantity,
                Note = item.Note
            };

            checkItems.Add(checkItem);

            // Count discrepancies
            if (checkItem.Difference != 0)
            {
                totalDiscrepancies++;
            }
        }

        // Update check status and add items
        check.Status = "COMPLETED";
        check.TotalDiscrepancies = totalDiscrepancies;
        
        // Add items to the existing collection instead of replacing it
        foreach (var item in checkItems)
        {
            check.InventoryCheckItems.Add(item);
        }

        await _inventoryCheckRepository.UpdateAsync(check);

        _logger.LogInformation("Submitted inventory check {CheckNumber} with {ItemCount} items and {DiscrepancyCount} discrepancies by user {UserId}", 
            check.CheckNumber, checkItems.Count, totalDiscrepancies, userContext.UserId);

        return await GetInventoryCheckByIdAsync(id) 
            ?? throw new InvalidOperationException(
                $"{ErrorCodes.InternalServerError}: Failed to retrieve updated inventory check");
    }

    // =====================================================
    // Step 3: Reconcile Differences (Read-only)
    // BACKEND RULE: MANAGER role required
    // =====================================================
    public async Task<IEnumerable<InventoryDiscrepancyDto>> ReconcileInventoryCheckAsync(Guid id, UserContext userContext)
    {
        _logger.LogInformation("User {UserId} (Role: {Role}) reconciling inventory check {Id}",
            userContext.UserId, userContext.Role, id);

        var check = await _inventoryCheckRepository.GetByIdAsync(id);
        if (check == null)
        {
            throw new KeyNotFoundException($"{ErrorCodes.InventoryCheckNotFound}: Inventory check {id} not found");
        }

        // BACKEND RULE 1: Must be completed to reconcile
        if (check.Status != "COMPLETED")
        {
            throw new InvalidOperationException($"{ErrorCodes.InvalidStateTransition}: Inventory check {id} must be COMPLETED to reconcile. Current status: {check.Status}");
        }

        var discrepancies = check.InventoryCheckItems
            .Where(item => item.Difference != 0)
            .Select(item => new InventoryDiscrepancyDto
            {
                ProductId = item.ProductId,
                SystemQuantity = item.SystemQuantity,
                ActualQuantity = item.ActualQuantity,
                Difference = item.Difference,
                Note = item.Note
            })
            .ToList();

        _logger.LogInformation("Found {DiscrepancyCount} discrepancies for inventory check {CheckNumber} by user {UserId}", 
            discrepancies.Count, check.CheckNumber, userContext.UserId);

        return discrepancies;
    }

    // =====================================================
    // Step 4: Approve Inventory Check
    // BACKEND RULE: MANAGER role required
    // =====================================================
    public async Task<InventoryCheckDto> ApproveInventoryCheckAsync(Guid id, ApproveInventoryCheckDto dto, UserContext userContext)
    {
        _logger.LogInformation("User {UserId} (Role: {Role}) approving inventory check {Id}",
            userContext.UserId, userContext.Role, id);

        var check = await _inventoryCheckRepository.GetByIdAsync(id);
        if (check == null)
        {
            throw new KeyNotFoundException($"{ErrorCodes.InventoryCheckNotFound}: Inventory check {id} not found");
        }

        // BACKEND RULE 1: Must be completed to approve
        if (check.Status != "COMPLETED")
        {
            throw new InvalidOperationException($"{ErrorCodes.InvalidStateTransition}: Inventory check {id} must be COMPLETED to approve. Current status: {check.Status}");
        }

        // Store approval info in notes (since schema doesn't have approved_by/approved_date)
        var approvalNote = $"[APPROVED by {userContext.UserId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC]";
        check.Notes = string.IsNullOrEmpty(check.Notes) 
            ? approvalNote 
            : $"{check.Notes}\n{approvalNote}";

        if (!string.IsNullOrEmpty(dto.Notes))
        {
            check.Notes += $"\nApproval Notes: {dto.Notes}";
        }

        await _inventoryCheckRepository.UpdateAsync(check);

        _logger.LogInformation("Approved inventory check {CheckNumber} by user {UserId}",
            check.CheckNumber, userContext.UserId);

        return await GetInventoryCheckByIdAsync(id) 
            ?? throw new InvalidOperationException($"{ErrorCodes.InternalServerError}: Failed to retrieve approved inventory check");
    }

    // =====================================================
    // Step 5: Adjust Inventory (Transaction-safe with Row Locking)
    // BACKEND RULES: MANAGER role, Transaction boundary, Row locking, Prevent double adjustment
    // =====================================================
    public async Task<InventoryCheckDto> AdjustInventoryAsync(Guid id, AdjustInventoryDto dto, UserContext userContext)
    {
        _logger.LogInformation("User {UserId} (Role: {Role}) adjusting inventory for check {Id}",
            userContext.UserId, userContext.Role, id);

        var check = await _inventoryCheckRepository.GetByIdAsync(id);
        if (check == null)
        {
            throw new KeyNotFoundException($"{ErrorCodes.InventoryCheckNotFound}: Inventory check {id} not found");
        }

        // BACKEND RULE 1: State machine - Must be COMPLETED
        if (check.Status != "COMPLETED")
        {
            throw new InvalidOperationException($"{ErrorCodes.InvalidStateTransition}: Inventory check {id} must be COMPLETED. Current status: {check.Status}");
        }

        // BACKEND RULE 2: Prevent double adjustment
        var checkState = InventoryCheckState.ParseFromNotes(check.Notes ?? string.Empty);
        if (checkState.IsAdjusted)
        {
            _logger.LogWarning("User {UserId} attempted to adjust already-adjusted inventory check {CheckNumber}",
                userContext.UserId, check.CheckNumber);
            throw new InvalidOperationException($"{ErrorCodes.InventoryCheckAlreadyAdjusted}: Inventory check {id} has already been adjusted");
        }

        // Verify that it's been approved first
        if (!checkState.IsApproved)
        {
            throw new InvalidOperationException($"{ErrorCodes.InventoryCheckNotApproved}: Inventory check {id} must be approved before adjustment");
        }

        // Get discrepancies
        var discrepancies = check.InventoryCheckItems
            .Where(item => item.Difference != 0)
            .ToList();

        if (!discrepancies.Any())
        {
            _logger.LogInformation("No discrepancies found for inventory check {CheckNumber}", check.CheckNumber);
            return await GetInventoryCheckByIdAsync(id) 
                ?? throw new InvalidOperationException($"{ErrorCodes.InternalServerError}: Failed to retrieve inventory check");
        }

        // BACKEND RULE 3: Begin database transaction
        await _inventoryLockingRepository.ExecuteInTransactionAsync(async () =>
        {
            // Generate movement number (for future implementation)
            // var movementNumber = await GenerateMovementNumberAsync();

            // TODO: Stock movement creation temporarily disabled due to schema mismatch
            // The stock_movements table structure doesn't match the StockMovement entity
            // Need to fix entity/mappings before enabling this feature

foreach (var item in discrepancies)
            {
                // BACKEND RULE 4: Row-level locking to prevent race conditions
                var inventory = await _inventoryLockingRepository.GetByLocationAndProductWithLockAsync(
                    check.LocationType,
                    check.LocationId,
                    item.ProductId
                );

                if (inventory == null)
                {
                    throw new KeyNotFoundException($"{ErrorCodes.InventoryNotFound}: Inventory record not found for product {item.ProductId} at location {check.LocationId}");
                }

                var oldQuantity = inventory.Quantity;
                var newQuantity = item.ActualQuantity;

                // Update inventory quantity
                inventory.Quantity = newQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                _inventoryLockingRepository.UpdateInventoryInTransaction(inventory);

                // BACKEND RULE 6: Log the adjustment (audit trail)
                // TODO: Temporarily disabled - InventoryLog entity doesn't match inventory_history table
                /*
                var log = new InventoryLog
                {
                    InventoryId = inventory.Id,
                    ProductId = item.ProductId,
                    Action = "ADJUST",
                    OldQuantity = oldQuantity,
                    NewQuantity = newQuantity,
                    Reason = $"Inventory check adjustment: {check.CheckNumber}. {dto.Reason ?? "Physical count discrepancy"}",
                    PerformedBy = userContext.UserId,
                    PerformedAt = DateTime.UtcNow
                };

                await _inventoryLogRepository.AddAsync(log);
                */

                _logger.LogInformation(
                    "Adjusted inventory for product {ProductId}: {OldQty} -> {NewQty} (diff: {Diff}) by user {UserId}",
                    item.ProductId, oldQuantity, newQuantity, item.Difference, userContext.UserId);
            }

            // Stock movement creation skipped (temporarily disabled)

            // Mark check as adjusted
            var adjustmentNote = $"[ADJUSTED by {userContext.UserId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC]";
            check.Notes = string.IsNullOrEmpty(check.Notes) 
                ? adjustmentNote 
                : $"{check.Notes}\n{adjustmentNote}";

            if (!string.IsNullOrEmpty(dto.Reason))
            {
                check.Notes += $"\nAdjustment Reason: {dto.Reason}";
            }

            await _inventoryCheckRepository.UpdateAsync(check);
        });

        _logger.LogInformation(
            "Successfully adjusted inventory for check {CheckNumber} by user {UserId}",
            check.CheckNumber, userContext.UserId);

        return await GetInventoryCheckByIdAsync(id) 
            ?? throw new InvalidOperationException($"{ErrorCodes.InternalServerError}: Failed to retrieve adjusted inventory check");
    }

    // =====================================================
    // Helper Methods
    // =====================================================

    private async Task<string> GenerateCheckNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var checks = await _inventoryCheckRepository.GetAllAsync();
        var prefix = $"IC-{year}-";
        var nextNumber = GetNextSequenceNumber(
            checks.Select(c => c.CheckNumber),
            prefix);

        return $"{prefix}{nextNumber:D3}";
    }

    private async Task<string> GenerateMovementNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var movements = await _stockMovementRepository.GetAllAsync();
        var prefix = $"SM-{year}-";
        var nextNumber = GetNextSequenceNumber(
            movements.Select(m => m.MovementNumber),
            prefix);

        return $"{prefix}{nextNumber:D3}";
    }

    private static int GetNextSequenceNumber(IEnumerable<string?> values, string prefix)
    {
        var max = 0;

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var suffix = value[prefix.Length..];
            if (int.TryParse(suffix, out var sequence) && sequence > max)
            {
                max = sequence;
            }
        }

        return max + 1;
    }

    private async Task ValidateLocationAsync(string locationType, Guid locationId)
    {
        var location = await _warehouseRepository.GetByIdAsync(locationId);
        if (location == null)
        {
            throw new KeyNotFoundException($"{locationType} with ID {locationId} not found");
        }

        if (location.IsDeleted)
        {
            throw new InvalidOperationException($"{locationType} {locationId} is deleted");
        }
    }

    private bool IsValidCheckType(string checkType)
    {
        return checkType is "FULL" or "PARTIAL" or "SPOT";
    }

    private bool IsValidLocationType(string locationType)
    {
        return locationType is "WAREHOUSE" or "STORE";
    }
}
