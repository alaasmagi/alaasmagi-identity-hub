namespace Web.Contracts.Responses;

/// <summary>
/// Response body for role synchronization.
/// </summary>
public sealed class SyncRolesResponseDto
{
    /// <summary>
    /// Gets or sets the role names from the sync request.
    /// </summary>
    public string[] SyncedRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets the configured default role name.
    /// </summary>
    public string? DefaultRole { get; set; }
}

/// <summary>
/// Response body for reading a user's client-scoped roles.
/// </summary>
public sealed class GetUserRolesResponseDto
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the role names scoped to the authenticated client.
    /// </summary>
    public string[] Roles { get; set; } = [];
}

/// <summary>
/// Response body for admin client create and update operations.
/// </summary>
public sealed class AdminClientResponseDto
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the public client identifier.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client display name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the configured allowed redirect URLs.
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the client is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the registration mode.
    /// </summary>
    public string RegistrationType { get; set; } = default!;

    /// <summary>
    /// Gets or sets the one-time plaintext secret for newly created clients.
    /// </summary>
    public string? ClientSecret { get; set; }
}
