using Application.Common;
using Application.Consent.Requests;
using Application.Consent.Responses;

namespace Application.Consent;

public interface IConsentService
{
    Task<Result<ConsentInfoResponse>> GetConsentInfoAsync(string consentToken);
    Task<Result<ConsentResponse>> GrantConsentAsync(GrantConsentRequest request);
    Task<Result<Unit>> RevokeConsentAsync(RevokeConsentRequest request);
}
