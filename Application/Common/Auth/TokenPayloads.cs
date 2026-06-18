using System.Text;
using System.Text.Json;

namespace Application.Common.Auth;

public sealed record RefreshTokenPayload(Guid UserId, Guid ClientId, string Value);
public sealed record TempTokenPayload(Guid UserId, Guid ClientId, string ResponseType, string Value, DateTime ExpiresAtUtc, string? RedirectUri = null);
public sealed record ConsentTokenPayload(Guid UserId, Guid ClientId, string ResponseType, string Value, DateTime ExpiresAtUtc, string? RedirectUri = null);

public static class TokenPayloads
{
    public static string Protect<T>(T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static T? Unprotect<T>(string token)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }
}
