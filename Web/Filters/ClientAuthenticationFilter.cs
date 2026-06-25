using Contracts.DataAccess;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Web.Contracts.Responses;

namespace Web.Filters;

/// <summary>
/// Authenticates machine-to-machine client requests with client credential headers.
/// </summary>
public sealed class ClientAuthenticationFilter : IAsyncActionFilter
{
    public const string AuthenticatedClientItemKey = "AuthenticatedClient";

    private readonly IClientRepository _clientRepository;
    private readonly PasswordHasher<ClientEntity> _passwordHasher = new();

    /// <summary>
    /// Initializes a new client authentication filter.
    /// </summary>
    /// <param name="clientRepository">The client repository.</param>
    public ClientAuthenticationFilter(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var headers = context.HttpContext.Request.Headers;
        if (!headers.TryGetValue("X-Client-Id", out var clientIdValues) ||
            !Guid.TryParse(clientIdValues.FirstOrDefault(), out var clientId) ||
            !headers.TryGetValue("X-Client-Secret", out var clientSecretValues))
        {
            context.Result = InvalidCredentials();
            return;
        }

        var clientSecret = clientSecretValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            context.Result = InvalidCredentials();
            return;
        }

        var client = await _clientRepository.GetByClientIdAsync(clientId);
        if (client is not { IsActive: true })
        {
            context.Result = InvalidCredentials();
            return;
        }

        var clientEntity = new ClientEntity
        {
            Id = client.Id,
            ClientId = client.ClientId,
            Name = client.Name,
            ClientSecretHash = client.ClientSecretHash,
            AllowedOrigins = client.AllowedOrigins,
            IsActive = client.IsActive,
            RegistrationType = client.RegistrationType,
            DefaultRoleId = client.DefaultRoleId
        };

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            clientEntity,
            client.ClientSecretHash,
            clientSecret);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            context.Result = InvalidCredentials();
            return;
        }

        context.HttpContext.Items[AuthenticatedClientItemKey] = client;
        await next();
    }

    private static JsonResult InvalidCredentials()
    {
        return new JsonResult(new ErrorResponse("InvalidClientCredentials"))
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }
}
