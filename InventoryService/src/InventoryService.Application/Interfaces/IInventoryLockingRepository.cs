using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

/// <summary>
/// Repository interface with locking support for inventory adjustments
/// </summary>
public interface IInventoryLockingRepository
{
    /// <summary>
    /// Get inventory record with exclusive update lock
    /// Uses: SELECT ... WITH (UPDLOCK, ROWLOCK)
    /// </summary>
    Task<Inventory?> GetByLocationAndProductWithLockAsync(
        string locationType, 
        Guid locationId, 
        Guid productId);
    
    /// <summary>
    /// Update inventory without calling SaveChanges (for transaction control)
    /// </summary>
    void UpdateInventoryInTransaction(Inventory inventory);
    
    /// <summary>
    /// Execute a function within a database transaction boundary
    /// Automatically commits on success or rolls back on exception
    /// </summary>
    Task ExecuteInTransactionAsync(Func<Task> operation);
}