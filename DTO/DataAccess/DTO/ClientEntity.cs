using Base.Domain;
using Domain;

namespace DTO.DataAccess.DTO;

public class ClientEntity : BaseEntityWithMetaConcurrency
{
    public string Name { get; set; } = default!;
    public Guid ClientId { get; set; }
    public string ClientSecretHash { get; set; } = default!;
    public string? AllowedOrigins { get; set; }
    public bool IsActive { get; set; }
    public ERegistrationType RegistrationType { get; set; }

    public ICollection<AppUserClientEntity>? UserClients { get; set; }
    public ICollection<AppRoleEntity>? Roles { get; set; }
}