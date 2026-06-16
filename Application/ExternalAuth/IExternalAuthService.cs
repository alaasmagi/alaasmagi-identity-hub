using Application.Common;
using Application.ExternalAuth.Requests;
using Application.ExternalAuth.Responses;

namespace Application.ExternalAuth;

public interface IExternalAuthService
{
    Task<Result<ProvidersResponse>> GetProvidersAsync(Guid clientId);
    Task<Result<ExternalCallbackResponse>> HandleExternalCallbackAsync(ExternalCallbackRequest request);
    Task<Result<ClaimsResponse>> ExchangeAuthCodeAsync(ExchangeCodeRequest request);
}
