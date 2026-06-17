namespace Application.Auth.Requests;

/// <summary>
/// Request to register a new local user account.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
/// <param name="FullName">The user's full name.</param>
public sealed record RegisterRequest(string Email, string Password, string FullName);

/// <summary>
/// Request to authenticate a local user for a client.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
/// <param name="ClientId">The target client identifier.</param>
/// <param name="ResponseType">The requested response type.</param>
public sealed record LoginRequest(string Email, string Password, Guid ClientId, string ResponseType);

/// <summary>
/// Request to rotate a refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token to rotate.</param>
/// <param name="ClientId">The target client identifier.</param>
public sealed record RefreshTokenRequest(string RefreshToken, Guid ClientId);

/// <summary>
/// Request to log out and revoke a refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token to revoke.</param>
/// <param name="UserId">The authenticated user identifier.</param>
public sealed record LogoutRequest(string RefreshToken, Guid UserId);

/// <summary>
/// Request to confirm a user's email address.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="Token">The email confirmation token.</param>
public sealed record ConfirmEmailRequest(Guid UserId, string Token);

/// <summary>
/// Request to start password reset.
/// </summary>
/// <param name="Email">The user's email address.</param>
public sealed record PasswordResetRequest(string Email);

/// <summary>
/// Request to confirm password reset.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="Token">The password reset token.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);

/// <summary>
/// Request to change the authenticated user's password.
/// </summary>
/// <param name="UserId">The authenticated user identifier.</param>
/// <param name="CurrentPassword">The current password.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record ChangePasswordRequest(Guid UserId, string CurrentPassword, string NewPassword);
