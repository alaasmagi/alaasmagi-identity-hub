using Application.ClientRoles.Requests;

namespace Web.Contracts.Requests;

/// <summary>
/// Request body for synchronizing a client's roles.
/// </summary>
public sealed class SyncRolesRequestDto
{
    /// <summary>
    /// Gets or sets the role definitions to synchronize.
    /// </summary>
    public List<RoleDefinitionDto>? Roles { get; set; } = [];
}

/// <summary>
/// Defines one client role in a sync request.
/// </summary>
public sealed class RoleDefinitionDto
{
    /// <summary>
    /// Gets or sets the role name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether this role is the client's default role.
    /// </summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// Request body for replacing a user's roles in the authenticated client.
/// </summary>
public sealed class SetUserRolesRequestDto
{
    /// <summary>
    /// Gets or sets the full replacement list of role names.
    /// </summary>
    public List<string>? Roles { get; set; } = [];
}

internal static class ClientManagementApiRequestMapping
{
    public static List<RoleDefinition> ToApplicationRoles(this IEnumerable<RoleDefinitionDto> roles)
    {
        return roles
            .Select(role => new RoleDefinition
            {
                Name = role.Name!,
                IsDefault = role.IsDefault
            })
            .ToList();
    }
}
