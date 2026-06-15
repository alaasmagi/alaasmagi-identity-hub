using Base.Contracts.DTO;
using Domain;
using DTO.DataAccess.DTO;

namespace DTO.DataAccess.Mapper;

public class AppRoleEntityMapper : IMapper<AppRole, AppRoleEntity>
{
    public AppRole? Map(AppRoleEntity? entity)
    {
        if (entity is null) return null;

        return new AppRole
        {
            Id = entity.Id,
            Name = entity.Name,
            NormalizedName = entity.NormalizedName,
            ConcurrencyStamp = entity.ConcurrencyStamp,
            ClientId = entity.ClientId
        };
    }

    public IEnumerable<AppRole> Map(IEnumerable<AppRoleEntity>? entities)
    {
        return entities?.Select(Map)!;
    }

    public AppRoleEntity? Map(AppRole? entity)
    {
        if (entity is null) return null;

        return new AppRoleEntity
        {
            Id = entity.Id,
            Name = entity.Name,
            NormalizedName = entity.NormalizedName,
            ConcurrencyStamp = entity.ConcurrencyStamp,
            ClientId = entity.ClientId
        };
    }

    public IEnumerable<AppRoleEntity> Map(IEnumerable<AppRole>? entities)
    {
        return entities?.Select(Map)!;
    }
}