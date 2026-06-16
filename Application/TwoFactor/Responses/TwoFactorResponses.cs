namespace Application.TwoFactor.Responses;

public sealed record TwoFactorStatusResponse(bool IsEnabled, bool HasAuthenticator, int RecoveryCodesLeft);
public sealed record AuthenticatorSetupResponse(string SharedKey, string QrCodeUri);
public sealed record RecoveryCodesResponse(IReadOnlyList<string> RecoveryCodes);
