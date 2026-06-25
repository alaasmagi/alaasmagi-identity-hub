namespace Application.ClientRoles.Responses;

public sealed record GetUserRolesResponse
{
    public Guid UserId { get; init; }
    public string[] Roles { get; init; } = [];
}
