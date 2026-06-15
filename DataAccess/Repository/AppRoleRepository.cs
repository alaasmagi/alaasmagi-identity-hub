using Base.DataAccess.EF;
using Contracts.DataAccess;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;

namespace DataAccess.Repository;

public class AppRoleRepository : BaseRepository<AppRole, AppRoleEntity, AppRoleEntityMapper>, IAppRoleRepository
{
    public AppRoleRepository(AppDbContext repositoryDbContext, AppRoleEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
    }
}