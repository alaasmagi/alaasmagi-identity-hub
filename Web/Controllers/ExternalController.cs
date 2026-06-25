using Application.ExternalAuth;
using Application.ExternalAuth.Requests;
using Application.ExternalAuth.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Web.Contracts.Requests;
using Web.Contracts.Responses;

namespace Web.Controllers;

/// <summary>
/// Handles external authentication endpoints.
/// </summary>
[ApiController]
[Route("api/auth/external")]
[Produces("application/json")]
[Tags("External")]
public sealed class ExternalController : ApiControllerBase
{
    private readonly IExternalAuthService _externalAuthService;

    /// <summary>
    /// Initializes a new external authentication controller.
    /// </summary>
    /// <param name="externalAuthService">The external authentication service.</param>
    public ExternalController(IExternalAuthService externalAuthService)
    {
        _externalAuthService = externalAuthService;
    }

    /// <summary>
    /// Gets external authentication providers for a client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>The configured external providers.</returns>
    [HttpGet("providers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProvidersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProviders([FromQuery] Guid clientId)
    {
        return HandleResult(await _externalAuthService.GetProvidersAsync(clientId));
    }

    /// <summary>
    /// Starts an external authentication challenge.
    /// </summary>
    /// <param name="request">The external challenge request body.</param>
    /// <returns>A challenge result for the external provider.</returns>
    [HttpPost("challenge")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult ChallengeExternal([FromBody] ExternalChallengeApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.RedirectUri))
        {
            return BadRequest(new ErrorResponse("InvalidExternalChallenge"));
        }

        var callbackUrl = Url.Action(
            nameof(Callback),
            "External",
            new
            {
                provider = request.Provider,
                clientId = request.ClientId,
                redirectUri = request.RedirectUri,
                tenantId = request.TenantId
            }) ?? "/api/auth/external/callback";

        var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
        properties.Items["clientId"] = request.ClientId.ToString();
        properties.Items["redirectUri"] = request.RedirectUri;
        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            properties.Items["tenant"] = request.TenantId;
            properties.Parameters["tenant"] = request.TenantId;
        }

        return Challenge(properties, request.Provider);
    }

    /// <summary>
    /// Handles an external authentication callback.
    /// </summary>
    /// <param name="provider">The external provider name.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="redirectUri">The redirect URI to return to.</param>
    /// <param name="tenantId">The optional Microsoft tenant identifier.</param>
    /// <returns>A redirect containing either an auth code or error.</returns>
    [HttpGet("callback")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Callback(
        [FromQuery] string provider,
        [FromQuery] Guid clientId,
        [FromQuery] string redirectUri,
        [FromQuery] string? tenantId)
    {
        var result = await _externalAuthService.HandleExternalCallbackAsync(
            new ExternalCallbackRequest(provider, clientId, redirectUri, tenantId));

        if (result.IsSuccess && result.Value is { RequiresConsent: true })
        {
            return Redirect(QueryHelpers.AddQueryString(redirectUri, "error", "ConsentRequired"));
        }

        if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Value?.Code))
        {
            return Redirect(QueryHelpers.AddQueryString(redirectUri, "code", result.Value.Code));
        }

        return Redirect(QueryHelpers.AddQueryString(redirectUri, "error", result.Error ?? "ExternalAuthenticationFailed"));
    }

    /// <summary>
    /// Exchanges an auth code for runtime claims.
    /// </summary>
    /// <param name="request">The auth-code exchange request body.</param>
    /// <returns>The runtime claims response.</returns>
    [HttpPost("token/exchange")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ClaimsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExchangeToken([FromBody] ExchangeCodeRequest request)
    {
        return HandleResult(await _externalAuthService.ExchangeAuthCodeAsync(request));
    }
}
