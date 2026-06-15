using Base.DataAccess.EF;
using Contracts.DataAccess;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;

namespace DataAccess.Repository;

public class AppUserClientRepository : BaseRepository<AppUserClient, AppUserClientEntity, AppUserClientEntityMapper>, IAppUserClientRepository
{
    public AppUserClientRepository(AppDbContext repositoryDbContext, AppUserClientEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
    }
}
