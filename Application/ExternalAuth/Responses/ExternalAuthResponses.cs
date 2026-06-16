using Application.Common;

namespace Application.ExternalAuth.Responses;

public sealed record ProvidersResponse(IReadOnlyList<string> Providers);
public sealed record ExternalCallbackResponse(string Code);
public sealed record ClaimsResponse(IReadOnlyList<ClaimDto> Claims);
