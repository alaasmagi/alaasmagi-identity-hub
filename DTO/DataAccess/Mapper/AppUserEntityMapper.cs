using Base.Contracts.DTO;
using Domain;
using DTO.DataAccess.DTO;

namespace DTO.DataAccess.Mapper;

public class AppUserEntityMapper : IMapper<AppUser, AppUserEntity>
{
    public AppUser? Map(AppUserEntity? entity)
    {
        if (entity is null) return null;

        return new AppUser
        {
            Id = entity.Id,
            UserName = entity.UserName,
            NormalizedUserName = entity.NormalizedUserName,
            Email = entity.Email,
            NormalizedEmail = entity.NormalizedEmail,
            EmailConfirmed = entity.EmailConfirmed,
            PasswordHash = entity.PasswordHash,
            SecurityStamp = entity.SecurityStamp,
            ConcurrencyStamp = entity.ConcurrencyStamp,
            PhoneNumber = entity.PhoneNumber,
            PhoneNumberConfirmed = entity.PhoneNumberConfirmed,
            TwoFactorEnabled = entity.TwoFactorEnabled,
            LockoutEnd = entity.LockoutEnd,
            LockoutEnabled = entity.LockoutEnabled,
            AccessFailedCount = entity.AccessFailedCount,
            FullName = entity.FullName,
            ProfilePictureUrl = entity.ProfilePictureUrl,
            IsActive = entity.IsActive,
            IsBanned = entity.IsBanned,
            BanReason = entity.BanReason
        };
    }

    public IEnumerable<AppUser> Map(IEnumerable<AppUserEntity>? entities)
    {
        return entities?.Select(Map)!;
    }

    public AppUserEntity? Map(AppUser? entity)
    {
        if (entity is null) return null;

        return new AppUserEntity
        {
            Id = entity.Id,
            UserName = entity.UserName,
            NormalizedUserName = entity.NormalizedUserName,
            Email = entity.Email,
            NormalizedEmail = entity.NormalizedEmail,
            EmailConfirmed = entity.EmailConfirmed,
            PasswordHash = entity.PasswordHash,
            SecurityStamp = entity.SecurityStamp,
            ConcurrencyStamp = entity.ConcurrencyStamp,
            PhoneNumber = entity.PhoneNumber,
            PhoneNumberConfirmed = entity.PhoneNumberConfirmed,
            TwoFactorEnabled = entity.TwoFactorEnabled,
            LockoutEnd = entity.LockoutEnd,
            LockoutEnabled = entity.LockoutEnabled,
            AccessFailedCount = entity.AccessFailedCount,
            FullName = entity.FullName,
            ProfilePictureUrl = entity.ProfilePictureUrl,
            IsActive = entity.IsActive,
            IsBanned = entity.IsBanned,
            BanReason = entity.BanReason
        };
    }

    public IEnumerable<AppUserEntity> Map(IEnumerable<AppUser>? entities)
    {
        return entities?.Select(Map)!;
    }
}