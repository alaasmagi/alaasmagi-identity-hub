using Base.Domain;

namespace Domain;

public class Client : BaseEntity
{
    public string Name { get; set; } = default!;
    public Guid ClientId { get; set; }
    public string ClientSecretHash { get; set; } = default!;
    public string? AllowedOrigins { get; set; }
    public bool IsActive { get; set; }
    public ERegistrationType RegistrationType { get; set; }
    public Guid? DefaultRoleId { get; set; }
    public AppRole? DefaultRole { get; set; }

    public ICollection<AppUserClient>? UserClients { get; set; }
    public ICollection<AppRole>? Roles { get; set; }
}
