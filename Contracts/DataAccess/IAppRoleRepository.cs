using Base.Contracts.DataAccess;
using Domain;

namespace Contracts.DataAccess;

public interface IAppRoleRepository : IBaseRepository<AppRole>
{
    Task<List<AppRole>> GetByClientIdAsync(Guid clientId);
    Task<AppRole?> GetByNameAndClientIdAsync(string normalizedName, Guid clientId);
}
