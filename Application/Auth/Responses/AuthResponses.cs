namespace Application.Auth.Responses;

public sealed record RegisterResponse(Guid UserId);

public sealed class LoginResponse
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public bool RequiresTwoFactor { get; init; }
    public string? TempToken { get; init; }
    public bool RequiresConsent { get; init; }
    public string? ConsentToken { get; init; }
    public string? Error { get; init; }
}

public sealed record TokenResponse(string AccessToken, string RefreshToken);
