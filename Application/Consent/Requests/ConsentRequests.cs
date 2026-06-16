namespace Application.Consent.Requests;

public sealed record GrantConsentRequest(string ConsentToken, Guid UserId, string? IpAddress);
public sealed record RevokeConsentRequest(Guid UserId, Guid ClientId);
