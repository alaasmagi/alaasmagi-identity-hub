using Domain;

namespace Application.Common.Abstractions;

public interface ISecurityEventService
{
    Task LogAsync(ESecurityEventType type, AppUser user, Client? client, string? ip, string? userAgent);
}
