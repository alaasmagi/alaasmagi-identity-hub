using Base.DataAccess.EF;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;

namespace DataAccess.Repository;

public class AppUserRepository : BaseRepository<AppUser, AppUserEntity, AppUserEntityMapper>
{
    public AppUserRepository(AppDbContext repositoryDbContext, AppUserEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
    }
}