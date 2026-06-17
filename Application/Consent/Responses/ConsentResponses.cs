using Domain;

namespace Application.Consent.Responses;

/// <summary>
/// Response describing consent target information.
/// </summary>
/// <param name="ClientName">The client display name.</param>
/// <param name="RegistrationType">The client's registration type.</param>
public sealed record ConsentInfoResponse(string ClientName, ERegistrationType RegistrationType);

/// <summary>
/// Response returned after consent is granted or queued.
/// </summary>
public sealed class ConsentResponse
{
    /// <summary>
    /// Gets the issued access token when active access is granted.
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Gets the issued refresh token when active access is granted.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Gets the resulting user-client status.
    /// </summary>
    public EUserClientStatus? Status { get; init; }
}
