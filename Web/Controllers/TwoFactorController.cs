using Application.Auth.Responses;
using Application.TwoFactor;
using Application.TwoFactor.Requests;
using Application.TwoFactor.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Web.Contracts.Requests;
using Web.Contracts.Responses;

namespace Web.Controllers;

/// <summary>
/// Handles two-factor authentication endpoints.
/// </summary>
[ApiController]
[Route("api/auth/2fa")]
[Produces("application/json")]
[Tags("TwoFactor")]
public sealed class TwoFactorController : ApiControllerBase
{
    private readonly ITwoFactorService _twoFactorService;

    /// <summary>
    /// Initializes a new two-factor controller.
    /// </summary>
    /// <param name="twoFactorService">The two-factor service.</param>
    public TwoFactorController(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    /// <summary>
    /// Gets the authenticated user's two-factor status.
    /// </summary>
    /// <returns>The two-factor status.</returns>
    [HttpGet("status")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(TwoFactorStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatus()
    {
        if (!TryGetUserId(out var userId)) return MissingUserId();
        return HandleResult(await _twoFactorService.GetStatusAsync(userId));
    }

    /// <summary>
    /// Gets authenticator setup information.
    /// </summary>
    /// <returns>The authenticator setup response.</returns>
    [HttpGet("authenticator/setup")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(AuthenticatorSetupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetupAuthenticator()
    {
        if (!TryGetUserId(out var userId)) return MissingUserId();
        return HandleResult(await _twoFactorService.SetupAuthenticatorAsync(userId));
    }

    /// <summary>
    /// Enables authenticator-based two-factor authentication.
    /// </summary>
    /// <param name="request">The authenticator code request body.</param>
    /// <returns>The generated recovery codes.</returns>
    [HttpPost("authenticator/enable")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EnableAuthenticator([FromBody] AuthenticatorCodeApiRequest request)
    {
        if (!TryGetUserId(out var userId)) return MissingUserId();
        return HandleResult(await _twoFactorService.EnableTwoFactorAsync(new EnableTwoFactorRequest(userId, request.Code)));
    }

    /// <summary>
    /// Disables authenticator-based two-factor authentication.
    /// </summary>
    /// <param name="request">The authenticator code request body.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("authenticator/disable")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DisableAuthenticator([FromBody] AuthenticatorCodeApiRequest request)
    {
        if (!TryGetUserId(out var userId)) return MissingUserId();
        return HandleResult(await _twoFactorService.DisableTwoFactorAsync(new DisableTwoFactorRequest(userId, request.Code)));
    }

    /// <summary>
    /// Regenerates recovery codes.
    /// </summary>
    /// <param name="request">The authenticator code request body.</param>
    /// <returns>The generated recovery codes.</returns>
    [HttpPost("recovery-codes/regenerate")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegenerateRecoveryCodes([FromBody] AuthenticatorCodeApiRequest request)
    {
        if (!TryGetUserId(out var userId)) return MissingUserId();
        return HandleResult(await _twoFactorService.RegenerateRecoveryCodesAsync(new RegenerateCodesRequest(userId, request.Code)));
    }

    /// <summary>
    /// Completes login with an authenticator code.
    /// </summary>
    /// <param name="request">The two-factor login request body.</param>
    /// <returns>The login response.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] TwoFactorLoginRequest request)
    {
        return HandleResult(await _twoFactorService.LoginWithTwoFactorAsync(request));
    }

    /// <summary>
    /// Completes login with a recovery code.
    /// </summary>
    /// <param name="request">The recovery-code login request body.</param>
    /// <returns>The login response.</returns>
    [HttpPost("login/recovery")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> LoginWithRecoveryCode([FromBody] RecoveryLoginRequest request)
    {
        return HandleResult(await _twoFactorService.LoginWithRecoveryCodeAsync(request));
    }
}
