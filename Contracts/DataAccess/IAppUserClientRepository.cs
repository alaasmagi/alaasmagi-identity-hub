using Base.Contracts.DataAccess;
using Domain;

namespace Contracts.DataAccess;

public interface IAppUserClientRepository : IBaseRepository<AppUserClient>
{
    Task<AppUserClient?> GetByUserAndClientAsync(Guid userId, Guid clientId);
    Task<List<AppUserClient>> GetByUserIdAsync(Guid userId);
    Task<List<AppUserClient>> GetByClientIdAsync(Guid clientId);
    Task<AppUserClient> AddUserClientAsync(AppUserClient userClient);
    Task UpdateUserClientAsync(AppUserClient userClient);
}
