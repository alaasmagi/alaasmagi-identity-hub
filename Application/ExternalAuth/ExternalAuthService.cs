using System.Security.Claims;
using Application.Common;
using Application.Common.Abstractions;
using Application.Common.Auth;
using Application.Common.Validation;
using Application.ExternalAuth.Requests;
using Application.ExternalAuth.Responses;
using Contracts.DataAccess;
using Domain;
using DTO.DataAccess.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Application.ExternalAuth;

public sealed class ExternalAuthService : IExternalAuthService
{
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly SignInManager<AppUserEntity> _signInManager;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IAppUserClientRepository _userClientRepository;
    private readonly ITokenService _tokenService;
    private readonly ISecurityEventService _securityEventService;
    private readonly AuthWorkflow _authWorkflow;
    private readonly IValidator<ExternalCallbackRequest> _callbackValidator;
    private readonly IValidator<ExchangeCodeRequest> _exchangeValidator;

    public ExternalAuthService(
        UserManager<AppUserEntity> userManager,
        SignInManager<AppUserEntity> signInManager,
        IAuthenticationSchemeProvider schemeProvider,
        IAppUserClientRepository userClientRepository,
        ITokenService tokenService,
        ISecurityEventService securityEventService,
        AuthWorkflow authWorkflow,
        IValidator<ExternalCallbackRequest> callbackValidator,
        IValidator<ExchangeCodeRequest> exchangeValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _schemeProvider = schemeProvider;
        _userClientRepository = userClientRepository;
        _tokenService = tokenService;
        _securityEventService = securityEventService;
        _authWorkflow = authWorkflow;
        _callbackValidator = callbackValidator;
        _exchangeValidator = exchangeValidator;
    }

    public async Task<Result<ProvidersResponse>> GetProvidersAsync(Guid clientId)
    {
        var client = await _authWorkflow.GetActiveClientAsync(clientId);
        if (client is null) return Result<ProvidersResponse>.Failure("InvalidClient");

        var schemes = await _schemeProvider.GetAllSchemesAsync();
        var providers = schemes
            .Where(scheme => !string.IsNullOrWhiteSpace(scheme.DisplayName))
            .Select(scheme => scheme.Name)
            .ToList();

        return Result<ProvidersResponse>.Success(new ProvidersResponse(providers));
    }

    public async Task<Result<ExternalCallbackResponse>> HandleExternalCallbackAsync(ExternalCallbackRequest request)
    {
        var validation = await _callbackValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<ExternalCallbackResponse>.Failure(validation.ToErrorMessage());

        var client = await _authWorkflow.GetActiveClientAsync(request.ClientId);
        if (client is null) return Result<ExternalCallbackResponse>.Failure("InvalidClient");

        if (!AuthWorkflow.IsRedirectUriAllowed(client, request.RedirectUri))
        {
            return Result<ExternalCallbackResponse>.Failure("RedirectUriNotAllowed");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null || !string.Equals(info.LoginProvider, request.Provider, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ExternalCallbackResponse>.Failure("ExternalLoginNotFound");
        }

        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (user is null)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<ExternalCallbackResponse>.Failure("ExternalEmailMissing");
            }

            user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new AppUserEntity
                {
                    Email = email,
                    UserName = email,
                    FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedBy = email,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = email,
                    UpdatedAt = DateTime.UtcNow,
                    ConcurrencyToken = Guid.NewGuid().ToString("N")
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Result<ExternalCallbackResponse>.Failure("ExternalUserCreationFailed");
                }
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                return Result<ExternalCallbackResponse>.Failure("ExternalLoginLinkFailed");
            }
        }

        if (!user.IsActive || user.IsBanned)
        {
            await _securityEventService.LogAsync(ESecurityEventType.FailedAttempt, user.ToDomainUser(), client, null, null);
            return Result<ExternalCallbackResponse>.Failure("InvalidCredentials");
        }

        var userClient = await _userClientRepository.GetByUserAndClientAsync(user.Id, client.ClientId);
        if (userClient is null)
        {
            var consentToken = await _authWorkflow.CreateConsentTokenAsync(user, client, "cookie", request.RedirectUri);
            return Result<ExternalCallbackResponse>.Success(new ExternalCallbackResponse(null, RequiresConsent: true, consentToken));
        }

        if (userClient.Status == EUserClientStatus.Pending) return Result<ExternalCallbackResponse>.Failure("AwaitingApproval");
        if (userClient.Status == EUserClientStatus.Revoked) return Result<ExternalCallbackResponse>.Failure("AccessRevoked");

        var code = _tokenService.GenerateAuthCode(user.Id.ToString(), client.ClientId.ToString(), request.RedirectUri);
        await _securityEventService.LogAsync(ESecurityEventType.Login, user.ToDomainUser(), client, null, null);

        return Result<ExternalCallbackResponse>.Success(new ExternalCallbackResponse(code));
    }

    public async Task<Result<ClaimsResponse>> ExchangeAuthCodeAsync(ExchangeCodeRequest request)
    {
        var validation = await _exchangeValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<ClaimsResponse>.Failure(validation.ToErrorMessage());

        var client = await _authWorkflow.GetActiveClientAsync(request.ClientId);
        if (client is null) return Result<ClaimsResponse>.Failure("InvalidClient");

        if (!IsClientSecretValid(client, request.ClientSecret))
        {
            return Result<ClaimsResponse>.Failure("InvalidClientSecret");
        }

        var payload = _tokenService.ValidateAuthCode(request.Code);
        if (payload is null || payload.ClientId != client.ClientId.ToString())
        {
            return Result<ClaimsResponse>.Failure("InvalidCode");
        }

        if (!AuthWorkflow.IsRedirectUriAllowed(client, payload.RedirectUri))
        {
            return Result<ClaimsResponse>.Failure("RedirectUriNotAllowed");
        }

        var user = await _userManager.FindByIdAsync(payload.UserId);
        if (user is null) return Result<ClaimsResponse>.Failure("InvalidCode");

        var userClient = await _userClientRepository.GetByUserAndClientAsync(user.Id, client.ClientId);
        if (userClient?.Status != EUserClientStatus.Active)
        {
            return Result<ClaimsResponse>.Failure("AccessNotActive");
        }

        var claims = await _authWorkflow.BuildRuntimeClaimsAsync(user, client);
        return Result<ClaimsResponse>.Success(new ClaimsResponse(claims));
    }

    private static bool IsClientSecretValid(Client client, string clientSecret)
    {
        if (string.Equals(client.ClientSecretHash, clientSecret, StringComparison.Ordinal))
        {
            return true;
        }

        var hasher = new PasswordHasher<Client>();
        return hasher.VerifyHashedPassword(client, client.ClientSecretHash, clientSecret) != PasswordVerificationResult.Failed;
    }
}
