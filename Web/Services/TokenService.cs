using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common;
using Application.Common.Abstractions;
using Application.Common.Auth;
using Domain;
using Microsoft.IdentityModel.Tokens;

namespace Web.Services;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(AppUser user, Client client, IList<string> roles)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var signingKey = GetSigningKey(jwtSection);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(GetConfiguredInt(jwtSection, "AccessTokenMinutes", 60));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("client_id", client.ClientId.ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string GenerateAuthCode(string userId, string clientId, string redirectUri)
    {
        return TokenPayloads.Protect(new AuthCodePayload(userId, clientId, redirectUri));
    }

    public AuthCodePayload? ValidateAuthCode(string code)
    {
        return TokenPayloads.Unprotect<AuthCodePayload>(code);
    }

    private static SymmetricSecurityKey GetSigningKey(IConfiguration jwtSection)
    {
        var signingKey = jwtSection["SigningKey"] ?? jwtSection["Key"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey or Jwt:Key must be configured.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    }

    private static int GetConfiguredInt(IConfiguration configuration, string key, int fallback)
    {
        return int.TryParse(configuration[key], out var value) ? value : fallback;
    }
}
