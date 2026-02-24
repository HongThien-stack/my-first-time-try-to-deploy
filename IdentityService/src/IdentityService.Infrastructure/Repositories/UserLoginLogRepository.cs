using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class UserLoginLogRepository : IUserLoginLogRepository
{
    private readonly IdentityDbContext _context;

    public UserLoginLogRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<UserLoginLog> CreateAsync(UserLoginLog log)
    {
        _context.UserLoginLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<IEnumerable<UserLoginLog>> GetByUserIdAsync(Guid userId, int limit = 50)
    {
        return await _context.UserLoginLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LoginAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserLoginLog>> GetRecentFailedLoginsAsync(Guid userId, int minutes = 30)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
        return await _context.UserLoginLogs
            .Where(l => l.UserId == userId && l.Status == "FAILED" && l.LoginAt >= cutoffTime)
            .OrderByDescending(l => l.LoginAt)
            .ToListAsync();
    }
}
