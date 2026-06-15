using Base.Domain;
using Domain;

namespace DTO.DataAccess.DTO;

public class SecurityEventEntity : BaseEntityUserWithMetaConcurrency
{
    public AppUserEntity? User { get; set; }
    public Guid? ClientId { get; set; }
    public ClientEntity? Client { get; set; }
    public ESecurityEventType Type { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}