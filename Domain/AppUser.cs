using Base.Domain;

namespace Domain;

public class AppUser : BaseIdentityUser
{
    public string FullName { get; set; } = default!;
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    public string? BanReason { get; set; }

    public ICollection<AppUserClient>? UserClients { get; set; }
}