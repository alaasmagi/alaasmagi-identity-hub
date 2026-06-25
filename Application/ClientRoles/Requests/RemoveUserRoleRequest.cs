namespace Application.ClientRoles.Requests;

public sealed record RemoveUserRoleRequest
{
    public Guid ClientDbId { get; init; }
    public Guid UserId { get; init; }
    public string RoleName { get; init; } = default!;
}
