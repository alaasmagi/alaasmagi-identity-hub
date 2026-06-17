namespace Application.Consent.Requests;

/// <summary>
/// Request to grant client consent.
/// </summary>
/// <param name="ConsentToken">The consent token.</param>
/// <param name="UserId">The user identifier derived from the consent context.</param>
/// <param name="IpAddress">The caller IP address.</param>
public sealed record GrantConsentRequest(string ConsentToken, Guid UserId, string? IpAddress);

/// <summary>
/// Request to revoke consent for a client.
/// </summary>
/// <param name="UserId">The authenticated user identifier.</param>
/// <param name="ClientId">The target client identifier.</param>
public sealed record RevokeConsentRequest(Guid UserId, Guid ClientId);
