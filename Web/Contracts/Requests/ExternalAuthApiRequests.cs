namespace Web.Contracts.Requests;

/// <summary>
/// Request body for starting an external authentication challenge.
/// </summary>
public sealed class ExternalChallengeApiRequest
{
    /// <summary>
    /// Gets or sets the external provider name.
    /// </summary>
    public string Provider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Gets or sets the redirect URI to return to after external authentication.
    /// </summary>
    public string RedirectUri { get; set; } = default!;

    /// <summary>
    /// Gets or sets the optional Microsoft tenant identifier.
    /// </summary>
    public string? TenantId { get; set; }
}
