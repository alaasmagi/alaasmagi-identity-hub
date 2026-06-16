namespace Application.Auth.Requests;

public sealed record RegisterRequest(string Email, string Password, string FullName);
public sealed record LoginRequest(string Email, string Password, Guid ClientId, string ResponseType);
public sealed record RefreshTokenRequest(string RefreshToken, Guid ClientId);
public sealed record LogoutRequest(string RefreshToken, Guid UserId);
public sealed record ConfirmEmailRequest(Guid UserId, string Token);
public sealed record PasswordResetRequest(string Email);
public sealed record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);
public sealed record ChangePasswordRequest(Guid UserId, string CurrentPassword, string NewPassword);
