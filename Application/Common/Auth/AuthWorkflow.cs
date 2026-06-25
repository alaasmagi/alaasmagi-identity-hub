using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Application.Auth.Responses;
using Application.Common.Abstractions;
using Contracts.DataAccess;
using Domain;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Common.Auth;

public sealed class AuthWorkflow
{
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly IClientRepository _clientRepository;
    private readonly IAppUserClientRepository _userClientRepository;
    private readonly IAppRoleRepository _roleRepository;
    private readonly ITokenService _tokenService;
    private readonly TokenLifetimeOptions _tokenLifetimeOptions;

    public AuthWorkflow(
        UserManager<AppUserEntity> userManager,
        IClientRepository clientRepository,
        IAppUserClientRepository userClientRepository,
        IAppRoleRepository roleRepository,
        ITokenService tokenService,
        IOptions<TokenLifetimeOptions> tokenLifetimeOptions)
    {
        _userManager = userManager;
        _clientRepository = clientRepository;
        _userClientRepository = userClientRepository;
        _roleRepository = roleRepository;
        _tokenService = tokenService;
        _tokenLifetimeOptions = tokenLifetimeOptions.Value;
    }

    public async Task<Client?> GetActiveClientAsync(Guid clientId)
    {
        var client = await _clientRepository.GetByClientIdAsync(clientId);
        return client is { IsActive: true } ? client : null;
    }

    public async Task<LoginResponse> ContinueAfterAuthenticationAsync(
        AppUserEntity user,
        Client client,
        string responseType,
        string? redirectUri = null)
    {
        var userClient = await _userClientRepository.GetByUserAndClientAsync(user.Id, client.ClientId);

        if (userClient is null)
        {
            var consentToken = await CreateConsentTokenAsync(user, client, responseType, redirectUri);
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
            EUserClientStatus.Active => await IssueLoginResponseAsync(user, client, responseType, redirectUri),
            _ => new LoginResponse { Error = "InvalidUserClientStatus" }
        };
    }

    public async Task<LoginResponse> IssueLoginResponseAsync(
        AppUserEntity user,
        Client client,
        string responseType = "jwt",
        string? redirectUri = null)
    {
        if (responseType == "cookie")
        {
            if (string.IsNullOrWhiteSpace(redirectUri) || !IsRedirectUriAllowed(client, redirectUri))
            {
                return new LoginResponse { Error = "InvalidRedirectUri" };
            }

            return new LoginResponse
            {
                AuthCode = _tokenService.GenerateAuthCode(user.Id.ToString(), client.ClientId.ToString(), redirectUri)
            };
        }

        var roles = await GetClientRolesAsync(user, client);
        var accessToken = _tokenService.GenerateAccessToken(user.ToDomainUser(), client, roles);
        var refreshToken = await CreateRefreshTokenAsync(user, client);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<IList<string>> GetClientRolesAsync(AppUserEntity user, Client client)
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

    public async Task<IReadOnlyList<ClaimDto>> BuildRuntimeClaimsAsync(AppUserEntity user, Client client)
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

    public async Task<string> CreateRefreshTokenAsync(AppUserEntity user, Client client)
    {
        var payload = new RefreshTokenPayload(user.Id, client.ClientId, _tokenService.GenerateRefreshToken());
        var token = TokenPayloads.Protect(payload);
        var stored = JsonSerializer.Serialize(new RefreshTokenStoragePayload(
            token,
            DateTime.UtcNow.AddSeconds(_tokenLifetimeOptions.RefreshTokenSeconds)));

        await _userManager.SetAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, RefreshTokenName(client.ClientId), stored);
        return token;
    }

    public async Task<Result<Unit>> ValidateRefreshTokenAsync(AppUserEntity user, Guid clientId, string refreshToken)
    {
        var stored = await _userManager.GetAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, RefreshTokenName(clientId));
        if (string.IsNullOrWhiteSpace(stored))
        {
            return Result<Unit>.Failure("InvalidRefreshToken");
        }

        var storedPayload = ParseRefreshTokenStoragePayload(stored);
        if (storedPayload is null)
        {
            return string.Equals(stored, refreshToken, StringComparison.Ordinal)
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure("InvalidRefreshToken");
        }

        if (storedPayload.ExpiresAt < DateTime.UtcNow)
        {
            return Result<Unit>.Failure("RefreshTokenExpired");
        }

        return string.Equals(storedPayload.Token, refreshToken, StringComparison.Ordinal)
            ? Result<Unit>.Success(Unit.Value)
            : Result<Unit>.Failure("InvalidRefreshToken");
    }

    public Task RevokeRefreshTokenAsync(AppUserEntity user, Guid clientId)
    {
        return _userManager.RemoveAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, RefreshTokenName(clientId));
    }

    public async Task RevokeAllRefreshTokensAsync(AppUserEntity user)
    {
        var clients = await _clientRepository.GetAllClientsAsync();
        foreach (var client in clients)
        {
            await RevokeRefreshTokenAsync(user, client.ClientId);
        }
    }

    public async Task<string> CreateTempTokenAsync(AppUserEntity user, Client client, string responseType, string? redirectUri = null)
    {
        var payload = new TempTokenPayload(
            user.Id,
            client.ClientId,
            responseType,
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow.Add(ApplicationTokenOptions.TempTokenLifetime),
            redirectUri);

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

    public Task ClearTempTokenAsync(AppUserEntity user, Guid clientId)
    {
        return _userManager.RemoveAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, TempTokenName(clientId));
    }

    public async Task<string> CreateConsentTokenAsync(AppUserEntity user, Client client, string responseType, string? redirectUri = null)
    {
        var payload = new ConsentTokenPayload(
            user.Id,
            client.ClientId,
            responseType,
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow.Add(ApplicationTokenOptions.ConsentTokenLifetime),
            redirectUri);

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

    public Task ClearConsentTokenAsync(AppUserEntity user, Guid clientId)
    {
        return _userManager.RemoveAuthenticationTokenAsync(user, ApplicationTokenOptions.Provider, ConsentTokenName(clientId));
    }

    public async Task AssignDefaultRoleIfPresentAsync(AppUserEntity user, Client client)
    {
        AppRole? role = client.DefaultRole;
        if (role is null && client.DefaultRoleId.HasValue)
        {
            var clientRoles = await _roleRepository.GetByClientIdAsync(client.Id);
            role = clientRoles.FirstOrDefault(clientRole => clientRole.Id == client.DefaultRoleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(role?.Name) && !await _userManager.IsInRoleAsync(user, role.Name))
        {
            await _userManager.AddToRoleAsync(user, role.Name);
        }
    }

    public static string RefreshTokenName(Guid clientId) => $"{ApplicationTokenOptions.RefreshTokenPrefix}:{clientId:N}";

    public static bool IsRedirectUriAllowed(Client client, string redirectUri)
    {
        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out _)) return false;

        var allowedOrigins = AllowedOriginsHelper.Parse(client.AllowedOrigins);
        return allowedOrigins.Contains(redirectUri, StringComparer.OrdinalIgnoreCase);
    }

    private static string TempTokenName(Guid clientId) => $"{ApplicationTokenOptions.TempTokenPrefix}:{clientId:N}";
    private static string ConsentTokenName(Guid clientId) => $"{ApplicationTokenOptions.ConsentTokenPrefix}:{clientId:N}";

    private static RefreshTokenStoragePayload? ParseRefreshTokenStoragePayload(string stored)
    {
        try
        {
            return JsonSerializer.Deserialize<RefreshTokenStoragePayload>(stored);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record RefreshTokenStoragePayload(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt);
}
