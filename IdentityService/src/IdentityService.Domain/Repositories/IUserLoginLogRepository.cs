using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IUserLoginLogRepository
{
    Task<UserLoginLog> CreateAsync(UserLoginLog log);
    Task<IEnumerable<UserLoginLog>> GetByUserIdAsync(Guid userId, int limit = 50);
    Task<IEnumerable<UserLoginLog>> GetRecentFailedLoginsAsync(Guid userId, int minutes = 30);
}
