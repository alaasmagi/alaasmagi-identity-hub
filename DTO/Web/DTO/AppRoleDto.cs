using Base.Domain;

namespace DTO.Web.DTO;

public class AppRoleDto : BaseEntity
{
    public string? Name { get; set; }
    public Guid ClientId { get; set; }
}