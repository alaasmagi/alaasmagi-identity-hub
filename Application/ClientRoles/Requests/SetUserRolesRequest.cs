namespace Application.ClientRoles.Requests;

public sealed record SetUserRolesRequest
{
    public Guid ClientDbId { get; init; }
    public Guid UserId { get; init; }
    public List<string> Roles { get; init; } = [];
}
