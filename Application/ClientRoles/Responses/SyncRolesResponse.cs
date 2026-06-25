namespace Application.ClientRoles.Responses;

public sealed record SyncRolesResponse
{
    public string[] SyncedRoles { get; init; } = [];
    public string? DefaultRole { get; init; }
}
