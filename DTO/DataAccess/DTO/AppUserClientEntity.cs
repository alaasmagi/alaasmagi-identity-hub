using Base.Domain;
using Domain;

namespace DTO.DataAccess.DTO;

public class AppUserClientEntity : BaseEntityUserWithMetaConcurrency
{
    public AppUserEntity? User { get; set; }

    public Guid ClientId { get; set; }
    public ClientEntity? Client { get; set; }

    public EUserClientStatus Status { get; set; }
    public DateTime GrantedAt { get; set; }
    public string? GrantedBy { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public string? RevokeReason { get; set; }
    public DateTime? ConsentGivenAt { get; set; }
    public string? ConsentIp { get; set; }
}