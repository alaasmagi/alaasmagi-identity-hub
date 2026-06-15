using Base.Domain;

namespace Domain;

public class AppUserClient : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    
    public EUserClientStatus Status { get; set; }
    public DateTime GrantedAt { get; set; }
    public string? GrantedBy { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public string? RevokeReason { get; set; }
    public DateTime? ConsentGivenAt { get; set; }
    public string? ConsentIp { get; set; }
}