using Base.Domain;

namespace DTO.DataAccess.DTO;

public class AppRoleEntity : BaseIdentityRoleWithMetaConcurrency
{
    public Guid ClientId { get; set; }
    public ClientEntity? Client { get; set; }
}