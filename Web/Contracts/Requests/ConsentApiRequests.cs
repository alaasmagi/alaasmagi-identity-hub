namespace Web.Contracts.Requests;

/// <summary>
/// Request body for granting client consent.
/// </summary>
public sealed class GrantConsentApiRequest
{
    /// <summary>
    /// Gets or sets the consent token.
    /// </summary>
    public string ConsentToken { get; set; } = default!;
}
