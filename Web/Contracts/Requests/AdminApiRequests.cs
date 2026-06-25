namespace Web.Contracts.Requests;

using Domain;

/// <summary>
/// Request body for banning a user.
/// </summary>
public sealed class BanUserApiRequest
{
    /// <summary>
    /// Gets or sets the ban reason.
    /// </summary>
    public string Reason { get; set; } = default!;
}

/// <summary>
/// Request body for creating an admin-managed client.
/// </summary>
public sealed class CreateClientApiRequest
{
    /// <summary>
    /// Gets or sets the client display name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the exact allowed redirect URLs for this client.
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the client is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the registration mode.
    /// </summary>
    public ERegistrationType RegistrationType { get; set; }
}

/// <summary>
/// Request body for updating an admin-managed client.
/// </summary>
public sealed class UpdateClientApiRequest
{
    /// <summary>
    /// Gets or sets the client display name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the exact allowed redirect URLs for this client.
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the client is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the registration mode.
    /// </summary>
    public ERegistrationType RegistrationType { get; set; }
}
