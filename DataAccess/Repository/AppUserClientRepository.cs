using Base.DataAccess.EF;
using Contracts.DataAccess;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository;

public class AppUserClientRepository : BaseRepository<AppUserClient, AppUserClientEntity, AppUserClientEntityMapper>, IAppUserClientRepository
{
    private readonly AppDbContext _context;
    private readonly AppUserClientEntityMapper _mapper;

    public AppUserClientRepository(AppDbContext repositoryDbContext, AppUserClientEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
        _context = repositoryDbContext;
        _mapper = repositoryMapper;
    }

    public async Task<AppUserClient?> GetByUserAndClientAsync(Guid userId, Guid clientId)
    {
        var entity = await _context.AppUserClients
            .AsNoTracking()
            .Include(userClient => userClient.User)
            .FirstOrDefaultAsync(userClient => userClient.UserId == userId && userClient.ClientId == clientId);

        return _mapper.Map(entity);
    }

    public async Task<List<AppUserClient>> GetByUserIdAsync(Guid userId)
    {
        var entities = await _context.AppUserClients
            .AsNoTracking()
            .Include(userClient => userClient.User)
            .Where(userClient => userClient.UserId == userId)
            .ToListAsync();

        return _mapper.Map(entities).ToList();
    }

    public async Task<List<AppUserClient>> GetByClientIdAsync(Guid clientId)
    {
        var entities = await _context.AppUserClients
            .AsNoTracking()
            .Include(userClient => userClient.User)
            .Where(userClient => userClient.ClientId == clientId)
            .ToListAsync();

        return _mapper.Map(entities).ToList();
    }

    public async Task<AppUserClient> AddUserClientAsync(AppUserClient userClient)
    {
        var entity = _mapper.Map(userClient)!;
        _context.AppUserClients.Add(entity);
        await _context.SaveChangesAsync();

        return _mapper.Map(entity)!;
    }

    public async Task UpdateUserClientAsync(AppUserClient userClient)
    {
        var entity = await _context.AppUserClients
            .FirstOrDefaultAsync(existing => existing.Id == userClient.Id);

        if (entity is null)
        {
            return;
        }

        entity.Status = userClient.Status;
        entity.GrantedAt = userClient.GrantedAt;
        entity.GrantedBy = userClient.GrantedBy;
        entity.RevokedAt = userClient.RevokedAt;
        entity.RevokedBy = userClient.RevokedBy;
        entity.RevokeReason = userClient.RevokeReason;
        entity.ConsentGivenAt = userClient.ConsentGivenAt;
        entity.ConsentIp = userClient.ConsentIp;

        await _context.SaveChangesAsync();
    }
}
