using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class RestockRequestRepository : IRestockRequestRepository
{
    private readonly InventoryDbContext _context;

    public RestockRequestRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RestockRequest>> GetAllAsync()
    {
        return await _context.RestockRequests
            .Include(r => r.RestockRequestItems)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<RestockRequest?> GetByIdAsync(Guid id)
    {
        return await _context.RestockRequests
            .Include(r => r.RestockRequestItems)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<RestockRequest?> GetByRequestNumberAsync(string requestNumber)
    {
        return await _context.RestockRequests
            .Include(r => r.RestockRequestItems)
            .FirstOrDefaultAsync(r => r.RequestNumber == requestNumber);
    }

    public async Task<IEnumerable<RestockRequest>> GetByStoreIdAsync(Guid storeId)
    {
        return await _context.RestockRequests
            .Include(r => r.RestockRequestItems)
            .Where(r => r.StoreId == storeId)
            .OrderByDescending(r => r.RequestedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestockRequest>> GetByStatusAsync(string status)
    {
        return await _context.RestockRequests
            .Include(r => r.RestockRequestItems)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.RequestedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestockRequest>> GetByPriorityAsync(string priority)
    {
        return await _context.RestockRequests
            .Include(r => r.RestockRequestItems)
            .Where(r => r.Priority == priority)
            .OrderByDescending(r => r.RequestedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestockRequest>> GetPendingRequestsAsync()
    {
        return await _context.RestockRequests
            .Include(r => r.RestockRequestItems)
            .Where(r => r.Status == "PENDING" || r.Status == "APPROVED")
            .OrderBy(r => r.Priority == "URGENT" ? 0 : r.Priority == "HIGH" ? 1 : 2)
            .ThenBy(r => r.RequestedDate)
            .ToListAsync();
    }

    public async Task<RestockRequest> AddAsync(RestockRequest request)
    {
        request.CreatedAt = DateTime.UtcNow;
        _context.RestockRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task UpdateAsync(RestockRequest request)
    {
        request.UpdatedAt = DateTime.UtcNow;
        _context.RestockRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var request = await _context.RestockRequests.FindAsync(id);
        if (request != null)
        {
            _context.RestockRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
    }
}
