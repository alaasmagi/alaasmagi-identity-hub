using Application.Auth.Responses;
using Application.Common;
using Application.TwoFactor.Requests;
using Application.TwoFactor.Responses;

namespace Application.TwoFactor;

public interface ITwoFactorService
{
    Task<Result<TwoFactorStatusResponse>> GetStatusAsync(Guid userId);
    Task<Result<AuthenticatorSetupResponse>> SetupAuthenticatorAsync(Guid userId);
    Task<Result<RecoveryCodesResponse>> EnableTwoFactorAsync(EnableTwoFactorRequest request);
    Task<Result<Unit>> DisableTwoFactorAsync(DisableTwoFactorRequest request);
    Task<Result<RecoveryCodesResponse>> RegenerateRecoveryCodesAsync(RegenerateCodesRequest request);
    Task<Result<LoginResponse>> LoginWithTwoFactorAsync(TwoFactorLoginRequest request);
    Task<Result<LoginResponse>> LoginWithRecoveryCodeAsync(RecoveryLoginRequest request);
}
