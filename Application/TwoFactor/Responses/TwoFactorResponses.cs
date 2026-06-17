namespace Application.TwoFactor.Responses;

/// <summary>
/// Response describing the current two-factor authentication status.
/// </summary>
/// <param name="IsEnabled">Whether two-factor authentication is enabled.</param>
/// <param name="HasAuthenticator">Whether an authenticator key exists.</param>
/// <param name="RecoveryCodesLeft">The number of recovery codes remaining.</param>
public sealed record TwoFactorStatusResponse(bool IsEnabled, bool HasAuthenticator, int RecoveryCodesLeft);

/// <summary>
/// Response containing authenticator setup data.
/// </summary>
/// <param name="SharedKey">The authenticator shared key.</param>
/// <param name="QrCodeUri">The QR-code URI.</param>
public sealed record AuthenticatorSetupResponse(string SharedKey, string QrCodeUri);

/// <summary>
/// Response containing generated recovery codes.
/// </summary>
/// <param name="RecoveryCodes">The generated recovery codes.</param>
public sealed record RecoveryCodesResponse(IReadOnlyList<string> RecoveryCodes);
