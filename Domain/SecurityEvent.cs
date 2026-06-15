using Base.Domain;

namespace Domain;

public class SecurityEvent : BaseEntityUser
{
    public AppUser? User { get; set; }
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public ESecurityEventType Type { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}