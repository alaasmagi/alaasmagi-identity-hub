using Base.DataAccess.EF;
using Contracts.DataAccess;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository;

public class ClientRepository : BaseRepository<Client, ClientEntity, ClientEntityMapper>, IClientRepository
{
    private readonly AppDbContext _context;
    private readonly ClientEntityMapper _mapper;

    public ClientRepository(AppDbContext repositoryDbContext, ClientEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
        _context = repositoryDbContext;
        _mapper = repositoryMapper;
    }

    public async Task<Client?> GetByDatabaseIdAsync(Guid id)
    {
        var entity = await _context.Clients
            .AsNoTracking()
            .Include(client => client.DefaultRole)
            .FirstOrDefaultAsync(client => client.Id == id);

        return _mapper.Map(entity);
    }

    public async Task<Client?> GetByClientIdAsync(Guid clientId)
    {
        var entity = await _context.Clients
            .AsNoTracking()
            .Include(client => client.DefaultRole)
            .FirstOrDefaultAsync(client => client.ClientId == clientId);

        return _mapper.Map(entity);
    }

    public async Task<List<Client>> GetAllClientsAsync()
    {
        var entities = await _context.Clients
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map(entities).ToList();
    }

    public async Task UpdateClientAsync(Client client)
    {
        var entity = await _context.Clients
            .FirstOrDefaultAsync(existing => existing.Id == client.Id || existing.ClientId == client.ClientId);

        if (entity is null)
        {
            return;
        }

        entity.Name = client.Name;
        entity.ClientId = client.ClientId;
        entity.ClientSecretHash = client.ClientSecretHash;
        entity.AllowedOrigins = client.AllowedOrigins;
        entity.IsActive = client.IsActive;
        entity.RegistrationType = client.RegistrationType;
        entity.DefaultRoleId = client.DefaultRoleId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
