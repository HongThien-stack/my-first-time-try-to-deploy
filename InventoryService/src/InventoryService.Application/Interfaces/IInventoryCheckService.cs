using InventoryService.Application.DTOs;
using InventoryService.Application.Models;

namespace InventoryService.Application.Interfaces;

public interface IInventoryCheckService
{
    // Existing methods
    Task<IEnumerable<InventoryCheckListDto>> GetAllInventoryChecksAsync();
    Task<InventoryCheckDto?> GetInventoryCheckByIdAsync(Guid id);
    
    // Step 1: Create Inventory Check Session (STAFF role required)
    Task<InventoryCheckDto> CreateInventoryCheckAsync(CreateInventoryCheckDto dto, UserContext userContext);
    
    // Step 2: Submit Inventory Check Results (STAFF role required)
    Task<InventoryCheckDto> SubmitInventoryCheckAsync(Guid id, SubmitInventoryCheckDto dto, UserContext userContext);
    
    // Step 3: Reconcile Differences (Read-only, MANAGER role required)
    Task<IEnumerable<InventoryDiscrepancyDto>> ReconcileInventoryCheckAsync(Guid id, UserContext userContext);
    
    // Step 4: Approve Inventory Check (MANAGER role required)
    Task<InventoryCheckDto> ApproveInventoryCheckAsync(Guid id, ApproveInventoryCheckDto dto, UserContext userContext);
    
    // Step 5: Adjust Inventory (Transaction-safe, MANAGER role required)
    Task<InventoryCheckDto> AdjustInventoryAsync(Guid id, AdjustInventoryDto dto, UserContext userContext);
}
