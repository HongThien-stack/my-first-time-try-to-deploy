using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IInventoryHistoryRepository
{
    /// <summary>
    /// Get inventory history with optional filters and pagination
    /// </summary>
    Task<(IEnumerable<InventoryHistory> Items, int TotalCount)> GetHistoryAsync(
        int page = 1,
        int pageSize = 10,
        Guid? productId = null,
        Guid? locationId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);
    
    /// <summary>
    /// Get history for specific inventory ID with pagination
    /// </summary>
    Task<(IEnumerable<InventoryHistory> Items, int TotalCount)> GetHistoryByInventoryIdAsync(
        Guid inventoryId,
        int page = 1,
        int pageSize = 10);
    
    /// <summary>
    /// Get latest snapshot for each inventory with pagination
    /// </summary>
    Task<(IEnumerable<InventoryHistory> Items, int TotalCount)> GetLatestSnapshotsAsync(
        int page = 1,
        int pageSize = 10);
}
