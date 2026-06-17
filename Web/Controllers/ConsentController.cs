using Application.Common.Auth;
using Application.Consent;
using Application.Consent.Requests;
using Application.Consent.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Contracts.Requests;
using Web.Contracts.Responses;

namespace Web.Controllers;

/// <summary>
/// Handles client consent endpoints.
/// </summary>
[ApiController]
[Route("api/auth/consent")]
[Produces("application/json")]
[Tags("Consent")]
public sealed class ConsentController : ApiControllerBase
{
    private readonly IConsentService _consentService;

    /// <summary>
    /// Initializes a new consent controller.
    /// </summary>
    /// <param name="consentService">The consent service.</param>
    public ConsentController(IConsentService consentService)
    {
        _consentService = consentService;
    }

    /// <summary>
    /// Gets consent information for a token.
    /// </summary>
    /// <param name="consentToken">The consent token.</param>
    /// <returns>The consent information.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ConsentInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetConsentInfo([FromQuery] string consentToken)
    {
        return HandleResult(await _consentService.GetConsentInfoAsync(consentToken));
    }

    /// <summary>
    /// Grants client consent.
    /// </summary>
    /// <param name="request">The grant consent request body.</param>
    /// <returns>The consent response.</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ConsentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GrantConsent([FromBody] GrantConsentApiRequest request)
    {
        var payload = TokenPayloads.Unprotect<ConsentTokenPayload>(request.ConsentToken);
        if (payload is null)
        {
            return BadRequest(new ErrorResponse("InvalidConsentToken"));
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        return HandleResult(await _consentService.GrantConsentAsync(
            new GrantConsentRequest(request.ConsentToken, payload.UserId, ipAddress)));
    }

    /// <summary>
    /// Revokes consent for a client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>An empty success response.</returns>
    [HttpDelete("{clientId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeConsent([FromRoute] Guid clientId)
    {
        if (!TryGetUserId(out var userId)) return MissingUserId();
        return HandleResult(await _consentService.RevokeConsentAsync(new RevokeConsentRequest(userId, clientId)));
    }
}
