using Base.Domain;
using Domain;

namespace DTO.Web.DTO;

public class ClientDto : BaseEntity
{
    public string Name { get; set; } = default!;
    public Guid ClientId { get; set; }
    public string? AllowedOrigins { get; set; }
    public bool IsActive { get; set; }
    public ERegistrationType RegistrationType { get; set; }
}