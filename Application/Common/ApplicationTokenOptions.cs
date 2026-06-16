namespace Application.Common;

public static class ApplicationTokenOptions
{
    public const string Provider = "IdentityHub";
    public const string RefreshTokenPrefix = "RefreshToken";
    public const string TempTokenPrefix = "TempToken";
    public const string ConsentTokenPrefix = "ConsentToken";
    public const string ConsentPayloadPrefix = "ConsentPayload";
    public const string AuthCodeTokenPrefix = "AuthCode";
    public static readonly TimeSpan ConsentTokenLifetime = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan TempTokenLifetime = TimeSpan.FromMinutes(10);
}
