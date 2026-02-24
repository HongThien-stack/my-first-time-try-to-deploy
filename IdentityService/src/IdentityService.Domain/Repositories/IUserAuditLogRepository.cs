using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IUserAuditLogRepository
{
    Task<UserAuditLog> CreateAsync(UserAuditLog log);
    Task<IEnumerable<UserAuditLog>> GetByUserIdAsync(Guid userId, int limit = 100);
    Task<IEnumerable<UserAuditLog>> GetByPerformerIdAsync(Guid performerId, int limit = 100);
    Task<IEnumerable<UserAuditLog>> GetByActionAsync(string action, int limit = 100);
}
