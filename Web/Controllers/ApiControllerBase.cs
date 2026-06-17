using System.Security.Claims;
using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Web.Contracts.Responses;

namespace Web.Controllers;

/// <summary>
/// Provides shared API controller helpers.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Maps an Application result to a standardized HTTP response.
    /// </summary>
    /// <param name="result">The result returned by the Application service.</param>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <returns>An HTTP response representing the result.</returns>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is null || result.Value is Unit ? Ok() : Ok(result.Value);
        }

        var error = new ErrorResponse(result.Error ?? "UnknownError");
        return result.Error switch
        {
            "NotFound" => NotFound(error),
            "Unauthorized" or "AccessRevoked" or "InvalidClientSecret" => Unauthorized(error),
            "AwaitingApproval" or "NotInvited" => StatusCode(StatusCodes.Status403Forbidden, error),
            _ => BadRequest(error)
        };
    }

    /// <summary>
    /// Attempts to read the authenticated user id from the current principal.
    /// </summary>
    /// <param name="userId">The parsed user id when available.</param>
    /// <returns>True when the claim exists and contains a valid user id.</returns>
    protected bool TryGetUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }

    /// <summary>
    /// Creates the standard unauthorized response used when the user id claim is missing.
    /// </summary>
    /// <returns>An unauthorized response.</returns>
    protected IActionResult MissingUserId()
    {
        return Unauthorized(new ErrorResponse("Unauthorized"));
    }
}
