using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IInventoryLogRepository
{
    /// <summary>
    /// Get inventory logs with optional filters and pagination
    /// </summary>
    Task<(IEnumerable<InventoryLog> Items, int TotalCount)> GetLogsAsync(
        int page = 1,
        int pageSize = 10,
        Guid? inventoryId = null,
        Guid? productId = null,
        string? action = null,
        Guid? performedBy = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);
    
    /// <summary>
    /// Get logs for specific inventory ID with pagination
    /// </summary>
    Task<(IEnumerable<InventoryLog> Items, int TotalCount)> GetLogsByInventoryIdAsync(
        Guid inventoryId,
        int page = 1,
        int pageSize = 10);
    
    /// <summary>
    /// Add a new inventory log entry
    /// </summary>
    Task<InventoryLog> AddAsync(InventoryLog log);
}
