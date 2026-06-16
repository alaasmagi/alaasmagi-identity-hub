namespace Application.Common;

public sealed record AuthCodePayload(string UserId, string ClientId, string RedirectUri);
