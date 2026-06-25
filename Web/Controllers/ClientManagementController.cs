using Application.ClientRoles;
using Application.ClientRoles.Requests;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Web.Contracts.Requests;
using Web.Contracts.Responses;
using Web.Filters;

namespace Web.Controllers;

/// <summary>
/// Handles service-to-service client role management endpoints.
/// </summary>
[ApiController]
[Route("api/client")]
[Produces("application/json")]
[Tags("Client Management")]
public sealed class ClientManagementController : ApiControllerBase
{
    private readonly IClientRoleService _clientRoleService;

    /// <summary>
    /// Initializes a new client management controller.
    /// </summary>
    /// <param name="clientRoleService">The client role service.</param>
    public ClientManagementController(IClientRoleService clientRoleService)
    {
        _clientRoleService = clientRoleService;
    }

    /// <summary>
    /// Synchronizes the authenticated client's role definitions.
    /// </summary>
    /// <remarks>
    /// This endpoint requires `X-Client-Id` and `X-Client-Secret` headers. It does not use a JWT bearer token.
    /// </remarks>
    /// <param name="request">The role synchronization request body.</param>
    /// <returns>The synchronized role names and selected default role.</returns>
    [HttpPost("roles/sync")]
    [ServiceFilter(typeof(ClientAuthenticationFilter))]
    [ProducesResponseType(typeof(SyncRolesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SyncRoles([FromBody] SyncRolesRequestDto request)
    {
        var client = AuthenticatedClient();
        var result = await _clientRoleService.SyncRolesAsync(new SyncRolesRequest
        {
            ClientDbId = client.Id,
            Roles = request.Roles?.ToApplicationRoles()!
        });

        if (!result.IsSuccess) return HandleResult(result);

        return Ok(new SyncRolesResponseDto
        {
            SyncedRoles = result.Value!.SyncedRoles,
            DefaultRole = result.Value.DefaultRole
        });
    }

    /// <summary>
    /// Gets a user's roles within the authenticated client's scope.
    /// </summary>
    /// <remarks>
    /// This endpoint requires `X-Client-Id` and `X-Client-Secret` headers. It does not use a JWT bearer token.
    /// </remarks>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user's client-scoped roles.</returns>
    [HttpGet("users/{userId:guid}/roles")]
    [ServiceFilter(typeof(ClientAuthenticationFilter))]
    [ProducesResponseType(typeof(GetUserRolesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoles([FromRoute] Guid userId)
    {
        var client = AuthenticatedClient();
        var result = await _clientRoleService.GetUserRolesAsync(client.Id, userId);
        if (!result.IsSuccess) return HandleResult(result);

        return Ok(new GetUserRolesResponseDto
        {
            UserId = result.Value!.UserId,
            Roles = result.Value.Roles
        });
    }

    /// <summary>
    /// Replaces a user's roles within the authenticated client's scope.
    /// </summary>
    /// <remarks>
    /// This endpoint requires `X-Client-Id` and `X-Client-Secret` headers. It does not use a JWT bearer token.
    /// </remarks>
    /// <param name="userId">The user identifier.</param>
    /// <param name="request">The full replacement role list.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("users/{userId:guid}/roles")]
    [ServiceFilter(typeof(ClientAuthenticationFilter))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetUserRoles([FromRoute] Guid userId, [FromBody] SetUserRolesRequestDto request)
    {
        var client = AuthenticatedClient();
        return HandleResult(await _clientRoleService.SetUserRolesAsync(new SetUserRolesRequest
        {
            ClientDbId = client.Id,
            UserId = userId,
            Roles = request.Roles!
        }));
    }

    /// <summary>
    /// Removes one role from a user within the authenticated client's scope.
    /// </summary>
    /// <remarks>
    /// This endpoint requires `X-Client-Id` and `X-Client-Secret` headers. It does not use a JWT bearer token.
    /// </remarks>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleName">The role name to remove.</param>
    /// <returns>An empty success response.</returns>
    [HttpDelete("users/{userId:guid}/roles/{roleName}")]
    [ServiceFilter(typeof(ClientAuthenticationFilter))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUserRole([FromRoute] Guid userId, [FromRoute] string roleName)
    {
        var client = AuthenticatedClient();
        return HandleResult(await _clientRoleService.RemoveUserRoleAsync(new RemoveUserRoleRequest
        {
            ClientDbId = client.Id,
            UserId = userId,
            RoleName = roleName
        }));
    }

    private Client AuthenticatedClient()
    {
        return (Client)HttpContext.Items[ClientAuthenticationFilter.AuthenticatedClientItemKey]!;
    }
}
