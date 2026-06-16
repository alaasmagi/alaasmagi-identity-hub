using Base.Contracts.DTO;
using Domain;
using DTO.DataAccess.DTO;

namespace DTO.DataAccess.Mapper;

public class AppUserClientEntityMapper : IMapper<AppUserClient, AppUserClientEntity>
{
    public AppUserClient? Map(AppUserClientEntity? entity)
    {
        if (entity is null) return null;

        return new AppUserClient
        {
            Id = entity.Id,
            UserId = entity.UserId,
            User = entity.User is null
                ? null
                : new AppUser
                {
                    Id = entity.User.Id,
                    UserName = entity.User.UserName,
                    Email = entity.User.Email,
                    FullName = entity.User.FullName,
                    ProfilePictureUrl = entity.User.ProfilePictureUrl,
                    IsActive = entity.User.IsActive,
                    IsBanned = entity.User.IsBanned,
                    BanReason = entity.User.BanReason
                },
            ClientId = entity.ClientId,
            Status = entity.Status,
            GrantedAt = entity.GrantedAt,
            GrantedBy = entity.GrantedBy,
            RevokedAt = entity.RevokedAt,
            RevokedBy = entity.RevokedBy,
            RevokeReason = entity.RevokeReason,
            ConsentGivenAt = entity.ConsentGivenAt,
            ConsentIp = entity.ConsentIp
        };
    }

    public IEnumerable<AppUserClient> Map(IEnumerable<AppUserClientEntity>? entities)
    {
        return entities?.Select(Map)!;
    }

    public AppUserClientEntity? Map(AppUserClient? entity)
    {
        if (entity is null) return null;

        return new AppUserClientEntity
        {
            Id = entity.Id,
            UserId = entity.UserId,
            User = entity.User is null
                ? null
                : new AppUserEntity
                {
                    Id = entity.User.Id,
                    UserName = entity.User.UserName,
                    Email = entity.User.Email,
                    FullName = entity.User.FullName,
                    ProfilePictureUrl = entity.User.ProfilePictureUrl,
                    IsActive = entity.User.IsActive,
                    IsBanned = entity.User.IsBanned,
                    BanReason = entity.User.BanReason
                },
            ClientId = entity.ClientId,
            Status = entity.Status,
            GrantedAt = entity.GrantedAt,
            GrantedBy = entity.GrantedBy,
            RevokedAt = entity.RevokedAt,
            RevokedBy = entity.RevokedBy,
            RevokeReason = entity.RevokeReason,
            ConsentGivenAt = entity.ConsentGivenAt,
            ConsentIp = entity.ConsentIp
        };
    }

    public IEnumerable<AppUserClientEntity> Map(IEnumerable<AppUserClient>? entities)
    {
        return entities?.Select(Map)!;
    }
}
