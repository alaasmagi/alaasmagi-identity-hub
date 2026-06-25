using System.Text.Json;

namespace Application.Common.Auth;

public static class AllowedOriginsHelper
{
    public static IReadOnlyList<string> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static string Serialize(IEnumerable<string> origins)
    {
        return JsonSerializer.Serialize(origins.ToList());
    }
}
