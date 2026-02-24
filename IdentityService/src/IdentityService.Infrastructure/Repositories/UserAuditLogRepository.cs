using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class UserAuditLogRepository : IUserAuditLogRepository
{
    private readonly IdentityDbContext _context;

    public UserAuditLogRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<UserAuditLog> CreateAsync(UserAuditLog log)
    {
        _context.UserAuditLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<IEnumerable<UserAuditLog>> GetByUserIdAsync(Guid userId, int limit = 100)
    {
        return await _context.UserAuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.PerformedAt)
            .Take(limit)
            .Include(l => l.Performer)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserAuditLog>> GetByPerformerIdAsync(Guid performerId, int limit = 100)
    {
        return await _context.UserAuditLogs
            .Where(l => l.PerformedBy == performerId)
            .OrderByDescending(l => l.PerformedAt)
            .Take(limit)
            .Include(l => l.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserAuditLog>> GetByActionAsync(string action, int limit = 100)
    {
        return await _context.UserAuditLogs
            .Where(l => l.Action == action)
            .OrderByDescending(l => l.PerformedAt)
            .Take(limit)
            .Include(l => l.User)
            .Include(l => l.Performer)
            .ToListAsync();
    }
}
