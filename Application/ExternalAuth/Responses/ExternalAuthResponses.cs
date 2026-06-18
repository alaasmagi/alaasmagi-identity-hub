using Application.Common;

namespace Application.ExternalAuth.Responses;

/// <summary>
/// Response listing configured external providers.
/// </summary>
/// <param name="Providers">The configured external provider names.</param>
public sealed record ProvidersResponse(IReadOnlyList<string> Providers);

/// <summary>
/// Response returned after a successful external callback.
/// </summary>
/// <param name="Code">The generated auth code.</param>
/// <param name="RequiresConsent">Whether the external user must grant client consent before login completes.</param>
/// <param name="ConsentToken">The consent token when consent is required.</param>
public sealed record ExternalCallbackResponse(string? Code, bool RequiresConsent = false, string? ConsentToken = null);

/// <summary>
/// Response containing runtime claims for an external MVC service.
/// </summary>
/// <param name="Claims">The runtime claims.</param>
public sealed record ClaimsResponse(IReadOnlyList<ClaimDto> Claims);
