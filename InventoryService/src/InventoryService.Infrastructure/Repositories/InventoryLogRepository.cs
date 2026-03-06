using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class InventoryLogRepository : IInventoryLogRepository
{
    private readonly InventoryDbContext _context;

    public InventoryLogRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<InventoryLog> Items, int TotalCount)> GetLogsAsync(
        int page = 1,
        int pageSize = 10,
        Guid? inventoryId = null,
        Guid? productId = null,
        string? action = null,
        Guid? performedBy = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.InventoryLogs.AsQueryable();

        if (inventoryId.HasValue)
        {
            query = query.Where(l => l.InventoryId == inventoryId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(l => l.ProductId == productId.Value);
        }

        if (!string.IsNullOrEmpty(action))
        {
            // Support multiple actions separated by comma: "ADJUST,RECEIVE,TRANSFER"
            var actions = action.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(l => actions.Contains(l.Action));
        }

        if (performedBy.HasValue)
        {
            query = query.Where(l => l.PerformedBy == performedBy.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(l => l.PerformedAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(l => l.PerformedAt <= dateTo.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .OrderByDescending(l => l.PerformedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<InventoryLog> Items, int TotalCount)> GetLogsByInventoryIdAsync(
        Guid inventoryId,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.InventoryLogs
            .Where(l => l.InventoryId == inventoryId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.PerformedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

}
