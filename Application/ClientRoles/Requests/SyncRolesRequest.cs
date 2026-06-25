namespace Application.ClientRoles.Requests;

public sealed record SyncRolesRequest
{
    public Guid ClientDbId { get; init; }
    public List<RoleDefinition> Roles { get; init; } = [];
}

public sealed record RoleDefinition
{
    public string Name { get; init; } = default!;
    public bool IsDefault { get; init; }
}
