using Domain;

namespace Application.Common.Abstractions;

public interface ITokenService
{
    string GenerateAccessToken(AppUser user, Client client, IList<string> roles);
    string GenerateRefreshToken();
    string GenerateAuthCode(string userId, string clientId, string redirectUri);
    AuthCodePayload? ValidateAuthCode(string code);
}
