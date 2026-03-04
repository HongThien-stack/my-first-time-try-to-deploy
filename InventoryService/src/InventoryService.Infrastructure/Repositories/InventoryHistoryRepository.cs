using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class InventoryHistoryRepository : IInventoryHistoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryHistoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<InventoryHistory> Items, int TotalCount)> GetHistoryAsync(
        int page = 1,
        int pageSize = 10,
        Guid? productId = null,
        Guid? locationId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.InventoryHistory.AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(h => h.ProductId == productId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(h => h.LocationId == locationId.Value);
        }

        if (dateFrom.HasValue)
        {
            var dateFromOnly = DateOnly.FromDateTime(dateFrom.Value);
            query = query.Where(h => h.SnapshotDate >= dateFromOnly);
        }

        if (dateTo.HasValue)
        {
            var dateToOnly = DateOnly.FromDateTime(dateTo.Value);
            query = query.Where(h => h.SnapshotDate <= dateToOnly);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .OrderByDescending(h => h.SnapshotDate)
            .ThenByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<InventoryHistory> Items, int TotalCount)> GetHistoryByInventoryIdAsync(
        Guid inventoryId,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.InventoryHistory
            .Where(h => h.InventoryId == inventoryId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(h => h.SnapshotDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<InventoryHistory> Items, int TotalCount)> GetLatestSnapshotsAsync(
        int page = 1,
        int pageSize = 10)
    {
        // Get the latest snapshot date
        var latestDate = await _context.InventoryHistory
            .MaxAsync(h => (DateOnly?)h.SnapshotDate);

        if (!latestDate.HasValue)
        {
            return (Enumerable.Empty<InventoryHistory>(), 0);
        }

        var query = _context.InventoryHistory
            .Where(h => h.SnapshotDate == latestDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(h => h.ProductId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
