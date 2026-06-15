using Base.Domain;
using Domain;

namespace DTO.Web.DTO;

public class SecurityEventDto : BaseEntity
{
    public Guid? ClientId { get; set; }
    public ESecurityEventType Type { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}