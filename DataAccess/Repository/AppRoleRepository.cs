using Base.DataAccess.EF;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;

namespace DataAccess.Repository;

public class AppRoleRepository : BaseRepository<AppRole, AppRoleEntity, AppRoleEntityMapper>
{
    public AppRoleRepository(AppDbContext repositoryDbContext, AppRoleEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
    }
}