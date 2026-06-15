using Base.Contracts.DTO;
using Domain;
using DTO.Web.DTO;

namespace DTO.Web.Mapper;

public class AppRoleDtoMapper : IMapper<AppRoleDto, AppRole>
{
    public AppRoleDto? Map(AppRole? entity)
    {
        if (entity is null) return null;

        return new AppRoleDto
        {
            Name = entity.Name,
            ClientId = entity.ClientId
        };
    }

    public IEnumerable<AppRoleDto> Map(IEnumerable<AppRole>? entities)
    {
        return entities?.Select(Map)!;
    }

    public AppRole? Map(AppRoleDto? entity)
    {
        if (entity is null) return null;

        return new AppRole
        {
            Name = entity.Name,
            ClientId = entity.ClientId
        };
    }

    public IEnumerable<AppRole> Map(IEnumerable<AppRoleDto>? entities)
    {
        return entities?.Select(Map)!;
    }
}