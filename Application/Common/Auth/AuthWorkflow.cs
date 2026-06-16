using System.Security.Claims;
using Application.Auth.Responses;
using Application.Common.Abstractions;
using Contracts.DataAccess;
using Domain;
using Microsoft.AspNetCore.Identity;

namespace Application.Common.Auth;

public sealed class AuthWorkflow
{
    private const string DefaultRoleName = "DEFAULT";

    private readonly UserManager<AppUser> _userManager;
    private readonly IClientRepository _clientRepository;
    private readonly IAppUserClientRepository _userClientRepository;
    private readonly IAppRoleRepository _roleRepository;
    private readonly ITokenService _tokenService;

    public AuthWorkflow(
        UserManager<AppUser> userManager,
        IClientRepository clientRepository,
        IAppUserClientRepository userClientRepository,
        IAppRoleRepository roleRepository,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _clientRepository = clientRepository;
        _userClientRepository = userClientRepository;
        _roleRepository = roleRepository;
        _tokenService = tokenService;
    }

    public async Task<Client?> GetActiveClientAsync(Guid clientId)
    {
        var client = await _clientRepository.GetByClientIdAsync(clientId);
        return client is { IsActive: true } ? client : null;
    }

    public async Task<LoginResponse> ContinueAfterAuthenticationAsync(AppUser user, Client client, string responseType)
    {
        var userClient = await _userClientRepository.GetByUserAndClientAsync(user.Id, client.ClientId);

        if (userClient is null)
        {
            var consentToken = await CreateConsentTokenAsync(user, client, responseType);
            return new LoginResponse
            {
                RequiresConsent = true,
                ConsentToken = consentToken
            };
        }

        return userClient.Status switch
        {
            EUserClientStatus.Pending => new LoginResponse { Error = "AwaitingApproval" },
            EUserClientStatus.Revoked => new LoginResponse { Error = "AccessRevoked" },
            EUserClientStatus.Active => await IssueLoginResponseAsync(user, client),
            _ => new LoginResponse { Error = "InvalidUserClientStatus" }
        };
    }

    public async Task<LoginResponse> IssueLoginResponseAsync(AppUser user, Client client)
    {
        var roles = await GetClientRolesAsync(user, client);
        var accessToken = _tokenService.GenerateAccessToken(user, client, roles);
        var refreshToken = await CreateRefreshTokenAsync(user, client);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<IList<string>> GetClientRolesAsync(AppUser user, Client client)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var clientRoles = await _roleRepository.GetByClientIdAsync(client.ClientId);
        var clientRoleNames = clientRoles
            .Select(role => role.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return userRoles
            .Where(role => clientRoleNames.Contains(role))
            .ToList();
    }

    public async Task<IReadOnlyList<ClaimDto>> BuildRuntimeClaimsAsync(AppUser user, Client client)
    {
        var roles = await GetClientRolesAsync(user, client);
        var claims = new List<ClaimDto>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sub", user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("email", user.Email ?? string.Empty),
            new("aud", client.ClientId.ToString())
        };

        claims.AddRange(roles.Select(role => new ClaimDto(ClaimTypes.Role, role)));
        return claims;
    }

    public async Task<string> CreateRefreshTokenAsync(AppUser user, Client client)
    {
        var payload = new RefreshTokenPayload(user.Id, client.ClientId, _tokenService.GenerateRefreshToken());
        var token = TokenPayloads.Protect(payload);
        await _userManager.SetAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, RefreshTokenName(client.ClientId), token);
        return token;
    }

    public async Task<bool> ValidateRefreshTokenAsync(AppUser user, Guid clientId, string refreshToken)
    {
        var stored = await _userManager.GetAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, RefreshTokenName(clientId));
        return string.Equals(stored, refreshToken, StringComparison.Ordinal);
    }

    public Task RevokeRefreshTokenAsync(AppUser user, Guid clientId)
    {
        return _userManager.RemoveAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, RefreshTokenName(clientId));
    }

    public async Task RevokeAllRefreshTokensAsync(AppUser user)
    {
        var clients = await _clientRepository.GetAllClientsAsync();
        foreach (var client in clients)
        {
            await RevokeRefreshTokenAsync(user, client.ClientId);
        }
    }

    public async Task<string> CreateTempTokenAsync(AppUser user, Client client, string responseType)
    {
        var payload = new TempTokenPayload(
            user.Id,
            client.ClientId,
            responseType,
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow.Add(ApplicationTokenOptions.TempTokenLifetime));

        var token = TokenPayloads.Protect(payload);
        await _userManager.SetAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, TempTokenName(client.ClientId), token);
        return token;
    }

    public async Task<TempTokenPayload?> ValidateTempTokenAsync(string token)
    {
        var payload = TokenPayloads.Unprotect<TempTokenPayload>(token);
        if (payload is null || payload.ExpiresAtUtc < DateTime.UtcNow)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(payload.UserId.ToString());
        if (user is null)
        {
            return null;
        }

        var stored = await _userManager.GetAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, TempTokenName(payload.ClientId));
        return string.Equals(stored, token, StringComparison.Ordinal) ? payload : null;
    }

    public Task ClearTempTokenAsync(AppUser user, Guid clientId)
    {
        return _userManager.RemoveAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, TempTokenName(clientId));
    }

    public async Task<string> CreateConsentTokenAsync(AppUser user, Client client, string responseType)
    {
        var payload = new ConsentTokenPayload(
            user.Id,
            client.ClientId,
            responseType,
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow.Add(ApplicationTokenOptions.ConsentTokenLifetime));

        var token = TokenPayloads.Protect(payload);
        await _userManager.SetAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, ConsentTokenName(client.ClientId), token);
        return token;
    }

    public async Task<ConsentTokenPayload?> ValidateConsentTokenAsync(string token)
    {
        var payload = TokenPayloads.Unprotect<ConsentTokenPayload>(token);
        if (payload is null || payload.ExpiresAtUtc < DateTime.UtcNow)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(payload.UserId.ToString());
        if (user is null)
        {
            return null;
        }

        var stored = await _userManager.GetAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, ConsentTokenName(payload.ClientId));
        return string.Equals(stored, token, StringComparison.Ordinal) ? payload : null;
    }

    public Task ClearConsentTokenAsync(AppUser user, Guid clientId)
    {
        return _userManager.RemoveAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, ConsentTokenName(clientId));
    }

    public async Task AssignDefaultRoleIfPresentAsync(AppUser user, Client client)
    {
        var role = await _roleRepository.GetByNameAndClientIdAsync(DefaultRoleName, client.ClientId);
        if (!string.IsNullOrWhiteSpace(role?.Name) && !await _userManager.IsInRoleAsync(user, role.Name))
        {
            await _userManager.AddToRoleAsync(user, role.Name);
        }
    }

    public static string RefreshTokenName(Guid clientId) => $"{ApplicationTokenOptions.RefreshTokenPrefix}:{clientId:N}";
    private static string TempTokenName(Guid clientId) => $"{ApplicationTokenOptions.TempTokenPrefix}:{clientId:N}";
    private static string ConsentTokenName(Guid clientId) => $"{ApplicationTokenOptions.ConsentTokenPrefix}:{clientId:N}";
}
