using Domain;
using DTO.DataAccess.DTO;

namespace Application.Common.Auth;

public static class AppUserEntityExtensions
{
    public static AppUser ToDomainUser(this AppUserEntity user)
    {
        return new AppUser
        {
            Id = user.Id,
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            Email = user.Email,
            NormalizedEmail = user.NormalizedEmail,
            EmailConfirmed = user.EmailConfirmed,
            PasswordHash = user.PasswordHash,
            SecurityStamp = user.SecurityStamp,
            ConcurrencyStamp = user.ConcurrencyStamp,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount,
            FullName = user.FullName,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsActive = user.IsActive,
            IsBanned = user.IsBanned,
            BanReason = user.BanReason
        };
    }
}
