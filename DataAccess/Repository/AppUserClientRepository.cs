using Base.DataAccess.EF;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;

namespace DataAccess.Repository;

public class AppUserClientRepository : BaseRepository<AppUserClient, AppUserClientEntity, AppUserClientEntityMapper>
{
    public AppUserClientRepository(AppDbContext repositoryDbContext, AppUserClientEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
    }
}
