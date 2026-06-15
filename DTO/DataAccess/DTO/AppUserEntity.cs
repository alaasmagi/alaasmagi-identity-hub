using Base.Domain;

namespace DTO.DataAccess.DTO;

public class AppUserEntity : BaseIdentityUserWithMetaConcurrency
{
    public string FullName { get; set; } = default!;
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    public string? BanReason { get; set; }

    public ICollection<AppUserClientEntity>? UserClients { get; set; }
}