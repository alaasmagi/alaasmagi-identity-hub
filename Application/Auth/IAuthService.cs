using Application.Auth.Requests;
using Application.Auth.Responses;
using Application.Common;

namespace Application.Auth;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result<Unit>> LogoutAsync(LogoutRequest request);
    Task<Result<Unit>> ConfirmEmailAsync(ConfirmEmailRequest request);
    Task<Result<Unit>> RequestPasswordResetAsync(PasswordResetRequest request);
    Task<Result<Unit>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result<Unit>> ChangePasswordAsync(ChangePasswordRequest request);
}
