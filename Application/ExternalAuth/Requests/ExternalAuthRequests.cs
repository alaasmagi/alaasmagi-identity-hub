namespace Application.ExternalAuth.Requests;

/// <summary>
/// Request to handle an external authentication callback.
/// </summary>
/// <param name="Provider">The external provider name.</param>
/// <param name="ClientId">The target client identifier.</param>
/// <param name="RedirectUri">The redirect URI to return to.</param>
/// <param name="TenantId">The optional Microsoft tenant identifier.</param>
public sealed record ExternalCallbackRequest(string Provider, Guid ClientId, string RedirectUri, string? TenantId);

/// <summary>
/// Request to exchange an auth code for runtime claims.
/// </summary>
/// <param name="Code">The auth code.</param>
/// <param name="ClientId">The target client identifier.</param>
/// <param name="ClientSecret">The client secret.</param>
public sealed record ExchangeCodeRequest(string Code, Guid ClientId, string ClientSecret);
