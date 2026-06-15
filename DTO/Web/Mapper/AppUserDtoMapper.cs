using Base.Contracts.DTO;
using Domain;
using DTO.Web.DTO;

namespace DTO.Web.Mapper;

public class AppUserDtoMapper : IMapper<AppUserDto, AppUser>
{
    public AppUserDto? Map(AppUser? entity)
    {
        if (entity is null) return null;

        return new AppUserDto
        {
            UserName = entity.UserName,
            Email = entity.Email,
            FullName = entity.FullName,
            ProfilePictureUrl = entity.ProfilePictureUrl,
            IsActive = entity.IsActive,
            IsBanned = entity.IsBanned,
            BanReason = entity.BanReason
        };
    }

    public IEnumerable<AppUserDto> Map(IEnumerable<AppUser>? entities)
    {
        return entities?.Select(Map)!;
    }

    public AppUser? Map(AppUserDto? entity)
    {
        if (entity is null) return null;

        return new AppUser
        {
            UserName = entity.UserName,
            Email = entity.Email,
            FullName = entity.FullName,
            ProfilePictureUrl = entity.ProfilePictureUrl,
            IsActive = entity.IsActive,
            IsBanned = entity.IsBanned,
            BanReason = entity.BanReason
        };
    }

    public IEnumerable<AppUser> Map(IEnumerable<AppUserDto>? entities)
    {
        return entities?.Select(Map)!;
    }
}