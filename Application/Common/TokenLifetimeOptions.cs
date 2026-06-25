namespace Application.Common;

public sealed class TokenLifetimeOptions
{
    public const string SectionName = "TokenLifetime";

    public int AccessTokenSeconds { get; set; } = 900;

    public int RefreshTokenSeconds { get; set; } = 604800;
}
