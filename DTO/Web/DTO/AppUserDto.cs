using Base.Domain;

namespace DTO.Web.DTO;

public class AppUserDto : BaseEntity
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string FullName { get; set; } = default!;
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
}