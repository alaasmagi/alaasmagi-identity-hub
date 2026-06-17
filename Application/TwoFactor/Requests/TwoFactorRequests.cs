namespace Application.TwoFactor.Requests;

/// <summary>
/// Request to enable two-factor authentication.
/// </summary>
/// <param name="UserId">The authenticated user identifier.</param>
/// <param name="Code">The authenticator code.</param>
public sealed record EnableTwoFactorRequest(Guid UserId, string Code);

/// <summary>
/// Request to disable two-factor authentication.
/// </summary>
/// <param name="UserId">The authenticated user identifier.</param>
/// <param name="Code">The authenticator code.</param>
public sealed record DisableTwoFactorRequest(Guid UserId, string Code);

/// <summary>
/// Request to regenerate recovery codes.
/// </summary>
/// <param name="UserId">The authenticated user identifier.</param>
/// <param name="Code">The authenticator code.</param>
public sealed record RegenerateCodesRequest(Guid UserId, string Code);

/// <summary>
/// Request to complete login with a two-factor authenticator code.
/// </summary>
/// <param name="TempToken">The temporary login token.</param>
/// <param name="Code">The authenticator code.</param>
public sealed record TwoFactorLoginRequest(string TempToken, string Code);

/// <summary>
/// Request to complete login with a recovery code.
/// </summary>
/// <param name="TempToken">The temporary login token.</param>
/// <param name="RecoveryCode">The recovery code.</param>
public sealed record RecoveryLoginRequest(string TempToken, string RecoveryCode);
