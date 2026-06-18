using Application.Admin;
using Application.Admin.Requests;
using Application.Admin.Responses;
using Application.Common;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Contracts.Requests;
using Web.Contracts.Responses;

namespace Web.Controllers;

/// <summary>
/// Handles admin API endpoints.
/// </summary>
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
[Tags("Admin")]
[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
public sealed class AdminController : ApiControllerBase
{
    private readonly IAdminService _adminService;

    /// <summary>
    /// Initializes a new admin controller.
    /// </summary>
    /// <param name="adminService">The admin service.</param>
    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Lists users.
    /// </summary>
    /// <param name="page">The requested page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="search">The optional search text.</param>
    /// <returns>A paged user response.</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResponse<UserSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? search)
    {
        return HandleResult(await _adminService.GetUsersAsync(new GetUsersRequest(page, pageSize, search)));
    }

    /// <summary>
    /// Bans a user.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="request">The ban request body.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("users/{userId:guid}/ban")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BanUser([FromRoute] Guid userId, [FromBody] BanUserApiRequest request)
    {
        if (!TryGetUserId(out var adminUserId)) return MissingUserId();
        return HandleResult(await _adminService.BanUserAsync(new BanUserRequest(userId, adminUserId, request.Reason)));
    }

    /// <summary>
    /// Unbans a user.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <returns>An empty success response.</returns>
    [HttpDelete("users/{userId:guid}/ban")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnbanUser([FromRoute] Guid userId)
    {
        if (!TryGetUserId(out var adminUserId)) return MissingUserId();
        return HandleResult(await _adminService.UnbanUserAsync(new UnbanUserRequest(userId, adminUserId)));
    }

    /// <summary>
    /// Lists client access records for a user.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <returns>The user's client access records.</returns>
    [HttpGet("users/{userId:guid}/clients")]
    [ProducesResponseType(typeof(List<AppUserClient>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserClients([FromRoute] Guid userId)
    {
        return HandleResult(await _adminService.GetUserClientsAsync(userId));
    }

    /// <summary>
    /// Approves a pending user-client relationship.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="clientId">The target client identifier.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("users/{userId:guid}/clients/{clientId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveUserClient([FromRoute] Guid userId, [FromRoute] Guid clientId)
    {
        if (!TryGetUserId(out var adminUserId)) return MissingUserId();
        return HandleResult(await _adminService.ApproveUserClientAsync(
            new ApproveUserClientRequest(userId, clientId, adminUserId)));
    }

    /// <summary>
    /// Lists security events.
    /// </summary>
    /// <param name="userId">The optional user identifier filter.</param>
    /// <param name="clientId">The optional client identifier filter.</param>
    /// <param name="page">The requested page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <returns>A paged security-event response.</returns>
    [HttpGet("security-events")]
    [ProducesResponseType(typeof(PagedResponse<SecurityEvent>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSecurityEvents(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? clientId,
        [FromQuery] int page,
        [FromQuery] int pageSize)
    {
        return HandleResult(await _adminService.GetSecurityEventsAsync(
            new GetSecurityEventsRequest(userId, clientId, page, pageSize)));
    }
}
