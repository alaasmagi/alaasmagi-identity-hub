using Base.Contracts.DataAccess;
using Domain;

namespace Contracts.DataAccess;

public interface IClientRepository : IBaseRepository<Client>
{
    Task<Client?> GetByDatabaseIdAsync(Guid id);
    Task<Client?> GetByClientIdAsync(Guid clientId);
    Task<List<Client>> GetAllClientsAsync();
    Task UpdateClientAsync(Client client);
}
