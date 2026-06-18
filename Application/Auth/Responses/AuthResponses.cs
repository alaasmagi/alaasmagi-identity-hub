namespace Application.Auth.Responses;

/// <summary>
/// Response returned after successful registration.
/// </summary>
/// <param name="UserId">The registered user identifier.</param>
public sealed record RegisterResponse(Guid UserId);

/// <summary>
/// Response returned by login-style flows.
/// </summary>
public sealed class LoginResponse
{
    /// <summary>
    /// Gets the issued access token when authentication is complete.
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Gets the issued refresh token when authentication is complete.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Gets the issued auth code when a cookie/federated response is requested.
    /// </summary>
    public string? AuthCode { get; init; }

    /// <summary>
    /// Gets a value indicating whether two-factor authentication is required.
    /// </summary>
    public bool RequiresTwoFactor { get; init; }

    /// <summary>
    /// Gets the temporary token used to complete two-factor authentication.
    /// </summary>
    public string? TempToken { get; init; }

    /// <summary>
    /// Gets a value indicating whether client consent is required.
    /// </summary>
    public bool RequiresConsent { get; init; }

    /// <summary>
    /// Gets the consent token used to complete consent.
    /// </summary>
    public string? ConsentToken { get; init; }

    /// <summary>
    /// Gets the internal service error when present.
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// Response returned after token issuance or refresh.
/// </summary>
/// <param name="AccessToken">The issued access token.</param>
/// <param name="RefreshToken">The issued refresh token.</param>
public sealed record TokenResponse(string AccessToken, string RefreshToken);
