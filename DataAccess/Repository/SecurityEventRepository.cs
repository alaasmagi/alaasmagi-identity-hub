using Base.DataAccess.EF;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;

namespace DataAccess.Repository;

public class SecurityEventRepository : BaseRepository<SecurityEvent, SecurityEventEntity, SecurityEventEntityMapper>
{
    public SecurityEventRepository(AppDbContext repositoryDbContext, SecurityEventEntityMapper repositoryMapper)
        : base(repositoryDbContext, repositoryMapper)
    {
    }
}