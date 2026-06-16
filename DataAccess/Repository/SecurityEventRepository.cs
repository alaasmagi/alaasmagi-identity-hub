using Base.DataAccess.EF;
using Contracts.DataAccess;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository;

public class SecurityEventRepository : BaseRepository<SecurityEvent, SecurityEventEntity, SecurityEventEntityMapper>, ISecurityEventRepository
{
    private readonly AppDbContext _context;
    private readonly SecurityEventEntityMapper _mapper;

    public SecurityEventRepository(AppDbContext repositoryDbContext, SecurityEventEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
        _context = repositoryDbContext;
        _mapper = repositoryMapper;
    }

    public async Task<(List<SecurityEvent> Items, int TotalCount)> GetPagedAsync(Guid? userId, Guid? clientId, int page, int pageSize)
    {
        var query = _context.SecurityEvents.AsNoTracking().AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(securityEvent => securityEvent.UserId == userId.Value);
        }

        if (clientId.HasValue)
        {
            query = query.Where(securityEvent => securityEvent.ClientId == clientId.Value);
        }

        var totalCount = await query.CountAsync();
        var entities = await query
            .OrderByDescending(securityEvent => securityEvent.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (_mapper.Map(entities).ToList(), totalCount);
    }
}
