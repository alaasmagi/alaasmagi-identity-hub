using Base.Contracts.DataAccess;
using Domain;

namespace Contracts.DataAccess;

public interface IClientRepository : IBaseRepository<Client>
{
    Task<Client?> GetByClientIdAsync(Guid clientId);
    Task<List<Client>> GetAllClientsAsync();
}
