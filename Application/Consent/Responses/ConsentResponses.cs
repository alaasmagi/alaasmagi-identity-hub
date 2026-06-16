using Domain;

namespace Application.Consent.Responses;

public sealed record ConsentInfoResponse(string ClientName, ERegistrationType RegistrationType);

public sealed class ConsentResponse
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public EUserClientStatus? Status { get; init; }
}
