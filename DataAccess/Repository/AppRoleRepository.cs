using Base.DataAccess.EF;
using Contracts.DataAccess;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository;

public class AppRoleRepository : BaseRepository<AppRole, AppRoleEntity, AppRoleEntityMapper>, IAppRoleRepository
{
    private readonly AppDbContext _context;
    private readonly AppRoleEntityMapper _mapper;

    public AppRoleRepository(AppDbContext repositoryDbContext, AppRoleEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
        _context = repositoryDbContext;
        _mapper = repositoryMapper;
    }

    public async Task<List<AppRole>> GetByClientIdAsync(Guid clientId)
    {
        var entities = await _context.Roles
            .AsNoTracking()
            .Where(role => role.ClientId == clientId)
            .ToListAsync();

        return _mapper.Map(entities).ToList();
    }

    public async Task<AppRole?> GetByNameAndClientIdAsync(string normalizedName, Guid clientId)
    {
        var entity = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.NormalizedName == normalizedName && role.ClientId == clientId);

        return _mapper.Map(entity);
    }
}
