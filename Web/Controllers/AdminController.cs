using Application.Admin;
using Application.Admin.Requests;
using Application.Admin.Responses;
using Application.Common;
using Application.Common.Auth;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Initializes a new admin controller.
    /// </summary>
    /// <param name="adminService">The admin service.</param>
    /// <param name="dbContext">The application database context.</param>
    public AdminController(IAdminService adminService, AppDbContext dbContext)
    {
        _adminService = adminService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a client.
    /// </summary>
    /// <remarks>
    /// Supply <c>AllowedOrigins</c> as a JSON array of exact redirect URL strings. The controller serializes the list to the internal `Client.AllowedOrigins` JSON string before saving. The generated plaintext client secret is returned exactly once in this response.
    /// </remarks>
    /// <param name="request">The client create request.</param>
    /// <returns>The created client and one-time plaintext secret.</returns>
    [HttpPost("clients")]
    [ProducesResponseType(typeof(AdminClientResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ErrorResponse("NameRequired"));
        }

        var secret = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var clientId = Guid.NewGuid();
        var client = new ClientEntity
        {
            Id = clientId,
            Name = request.Name.Trim(),
            ClientId = clientId,
            ClientSecretHash = new PasswordHasher<ClientEntity>().HashPassword(null!, secret),
            AllowedOrigins = AllowedOriginsHelper.Serialize(NormalizeAllowedOrigins(request.AllowedOrigins)),
            IsActive = request.IsActive,
            RegistrationType = request.RegistrationType,
            CreatedBy = User.Identity?.Name ?? "admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = User.Identity?.Name ?? "admin",
            UpdatedAt = DateTime.UtcNow,
            ConcurrencyToken = Guid.NewGuid().ToString("N")
        };

        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateClient), ToResponse(client, secret));
    }

    /// <summary>
    /// Updates a client.
    /// </summary>
    /// <remarks>
    /// Supply <c>AllowedOrigins</c> as a JSON array of exact redirect URL strings. The controller serializes the list to the internal `Client.AllowedOrigins` JSON string before saving.
    /// </remarks>
    /// <param name="id">The database client identifier.</param>
    /// <param name="request">The client update request.</param>
    /// <returns>The updated client.</returns>
    [HttpPut("clients/{id:guid}")]
    [ProducesResponseType(typeof(AdminClientResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClient([FromRoute] Guid id, [FromBody] UpdateClientApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ErrorResponse("NameRequired"));
        }

        var client = await _dbContext.Clients.FirstOrDefaultAsync(existing => existing.Id == id);
        if (client is null)
        {
            return NotFound(new ErrorResponse("NotFound"));
        }

        client.Name = request.Name.Trim();
        client.AllowedOrigins = AllowedOriginsHelper.Serialize(NormalizeAllowedOrigins(request.AllowedOrigins));
        client.IsActive = request.IsActive;
        client.RegistrationType = request.RegistrationType;
        client.UpdatedBy = User.Identity?.Name ?? "admin";
        client.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(client));
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

    private static List<string> NormalizeAllowedOrigins(IEnumerable<string>? origins)
    {
        return (origins ?? [])
            .Select(origin => origin.Trim())
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToList();
    }

    private static AdminClientResponseDto ToResponse(ClientEntity client, string? clientSecret = null)
    {
        return new AdminClientResponseDto
        {
            Id = client.Id,
            ClientId = client.ClientId,
            Name = client.Name,
            AllowedOrigins = AllowedOriginsHelper.Parse(client.AllowedOrigins).ToList(),
            IsActive = client.IsActive,
            RegistrationType = client.RegistrationType.ToString(),
            ClientSecret = clientSecret
        };
    }
}
