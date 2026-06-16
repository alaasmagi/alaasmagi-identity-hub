namespace Application.ExternalAuth.Requests;

public sealed record ExternalCallbackRequest(string Provider, Guid ClientId, string RedirectUri, string? TenantId);
public sealed record ExchangeCodeRequest(string Code, Guid ClientId, string ClientSecret);
