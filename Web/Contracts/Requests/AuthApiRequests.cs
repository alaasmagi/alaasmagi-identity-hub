namespace Web.Contracts.Requests;

/// <summary>
/// Request body for logging out with a refresh token.
/// </summary>
public sealed class LogoutApiRequest
{
    /// <summary>
    /// Gets or sets the refresh token to revoke.
    /// </summary>
    public string RefreshToken { get; set; } = default!;
}

/// <summary>
/// Request body for changing the authenticated user's password.
/// </summary>
public sealed class ChangePasswordApiRequest
{
    /// <summary>
    /// Gets or sets the current password.
    /// </summary>
    public string CurrentPassword { get; set; } = default!;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    public string NewPassword { get; set; } = default!;
}
