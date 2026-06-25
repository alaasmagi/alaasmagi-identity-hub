using Application.Auth;
using Application.Auth.Requests;
using Application.Auth.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Web.Contracts.Requests;
using Web.Contracts.Responses;

namespace Web.Controllers;

/// <summary>
/// Handles local authentication endpoints.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[Tags("Auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new authentication controller.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration request body.</param>
    /// <returns>The registered user identifier.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Register), new { userId = result.Value?.UserId }, result.Value)
            : HandleResult(result);
    }

    /// <summary>
    /// Authenticates a user for a client.
    /// </summary>
    /// <param name="request">The login request body.</param>
    /// <returns>The login response.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        return HandleResult(await _authService.LoginAsync(request));
    }

    /// <summary>
    /// Logs out the authenticated user.
    /// </summary>
    /// <param name="request">The logout request body.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutApiRequest request)
    {
        if (!TryGetUserId(out var userId)) return MissingUserId();
        return HandleResult(await _authService.LogoutAsync(new LogoutRequest(request.RefreshToken, userId)));
    }

    /// <summary>
    /// Refreshes an access token.
    /// </summary>
    /// <param name="request">The refresh request body.</param>
    /// <returns>The refreshed tokens.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        return HandleResult(await _authService.RefreshTokenAsync(request));
    }

    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    /// <param name="request">The email confirmation request body.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("email/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        return HandleResult(await _authService.ConfirmEmailAsync(request));
    }

    /// <summary>
    /// Requests a password reset email.
    /// </summary>
    /// <param name="request">The password reset request body.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("password/reset/request")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
    {
        return HandleResult(await _authService.RequestPasswordResetAsync(request));
    }

    /// <summary>
    /// Confirms a password reset.
    /// </summary>
    /// <param name="request">The password reset confirmation request body.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("password/reset/confirm")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        return HandleResult(await _authService.ResetPasswordAsync(request));
    }

    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    /// <param name="request">The password change request body.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("password/change")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordApiRequest request)
    {
        if (!TryGetUserId(out var userId)) return MissingUserId();
        return HandleResult(await _authService.ChangePasswordAsync(
            new ChangePasswordRequest(userId, request.CurrentPassword, request.NewPassword)));
    }
}
