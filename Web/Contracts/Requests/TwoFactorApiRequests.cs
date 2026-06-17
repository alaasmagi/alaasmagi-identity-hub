namespace Web.Contracts.Requests;

/// <summary>
/// Request body containing an authenticator verification code.
/// </summary>
public sealed class AuthenticatorCodeApiRequest
{
    /// <summary>
    /// Gets or sets the authenticator code.
    /// </summary>
    public string Code { get; set; } = default!;
}
