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

    public async Task<Client?> GetByClientIdAsync(Guid clientId)
    {
        var entity = await _context.Clients
            .AsNoTracking()
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
}
