namespace Application.TwoFactor.Requests;

public sealed record EnableTwoFactorRequest(Guid UserId, string Code);
public sealed record DisableTwoFactorRequest(Guid UserId, string Code);
public sealed record RegenerateCodesRequest(Guid UserId, string Code);
public sealed record TwoFactorLoginRequest(string TempToken, string Code);
public sealed record RecoveryLoginRequest(string TempToken, string RecoveryCode);
