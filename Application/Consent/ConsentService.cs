using Application.Common;
using Application.Common.Abstractions;
using Application.Common.Auth;
using Application.Common.Validation;
using Application.Consent.Requests;
using Application.Consent.Responses;
using Contracts.DataAccess;
using Domain;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Consent;

public sealed class ConsentService : IConsentService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IAppUserClientRepository _userClientRepository;
    private readonly ISecurityEventService _securityEventService;
    private readonly AuthWorkflow _authWorkflow;
    private readonly IValidator<GrantConsentRequest> _grantConsentValidator;
    private readonly IValidator<RevokeConsentRequest> _revokeConsentValidator;

    public ConsentService(
        UserManager<AppUser> userManager,
        IAppUserClientRepository userClientRepository,
        ISecurityEventService securityEventService,
        AuthWorkflow authWorkflow,
        IValidator<GrantConsentRequest> grantConsentValidator,
        IValidator<RevokeConsentRequest> revokeConsentValidator)
    {
        _userManager = userManager;
        _userClientRepository = userClientRepository;
        _securityEventService = securityEventService;
        _authWorkflow = authWorkflow;
        _grantConsentValidator = grantConsentValidator;
        _revokeConsentValidator = revokeConsentValidator;
    }

    public async Task<Result<ConsentInfoResponse>> GetConsentInfoAsync(string consentToken)
    {
        if (string.IsNullOrWhiteSpace(consentToken))
        {
            return Result<ConsentInfoResponse>.Failure("InvalidConsentToken");
        }

        var payload = await _authWorkflow.ValidateConsentTokenAsync(consentToken);
        if (payload is null) return Result<ConsentInfoResponse>.Failure("InvalidConsentToken");

        var client = await _authWorkflow.GetActiveClientAsync(payload.ClientId);
        return client is null
            ? Result<ConsentInfoResponse>.Failure("InvalidClient")
            : Result<ConsentInfoResponse>.Success(new ConsentInfoResponse(client.Name, client.RegistrationType));
    }

    public async Task<Result<ConsentResponse>> GrantConsentAsync(GrantConsentRequest request)
    {
        var validation = await _grantConsentValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<ConsentResponse>.Failure(validation.ToErrorMessage());

        var payload = await _authWorkflow.ValidateConsentTokenAsync(request.ConsentToken);
        if (payload is null || payload.UserId != request.UserId)
        {
            return Result<ConsentResponse>.Failure("InvalidConsentToken");
        }

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        var client = await _authWorkflow.GetActiveClientAsync(payload.ClientId);
        if (user is null || client is null)
        {
            return Result<ConsentResponse>.Failure("InvalidConsentToken");
        }

        var existing = await _userClientRepository.GetByUserAndClientAsync(user.Id, client.ClientId);
        if (existing is { Status: EUserClientStatus.Active })
        {
            var loginResponse = await _authWorkflow.IssueLoginResponseAsync(user, client, payload.ResponseType, payload.RedirectUri);
            if (loginResponse.Error is not null) return Result<ConsentResponse>.Failure(loginResponse.Error);

            return Result<ConsentResponse>.Success(new ConsentResponse
            {
                AuthCode = loginResponse.AuthCode,
                AccessToken = loginResponse.AccessToken,
                RefreshToken = loginResponse.RefreshToken,
                Status = EUserClientStatus.Active
            });
        }

        if (client.RegistrationType == ERegistrationType.InviteOnly)
        {
            return Result<ConsentResponse>.Failure("NotInvited");
        }

        var status = client.RegistrationType == ERegistrationType.RequiresApproval
            ? EUserClientStatus.Pending
            : EUserClientStatus.Active;

        var userClient = existing ?? new AppUserClient
        {
            UserId = user.Id,
            ClientId = client.ClientId
        };

        userClient.Status = status;
        userClient.GrantedAt = DateTime.UtcNow;
        userClient.GrantedBy = user.Id.ToString();
        userClient.ConsentGivenAt = DateTime.UtcNow;
        userClient.ConsentIp = request.IpAddress;
        userClient.RevokedAt = null;
        userClient.RevokedBy = null;
        userClient.RevokeReason = null;

        if (existing is null)
        {
            await _userClientRepository.AddUserClientAsync(userClient);
        }
        else
        {
            await _userClientRepository.UpdateUserClientAsync(userClient);
        }

        await _authWorkflow.ClearConsentTokenAsync(user, client.ClientId);
        await _securityEventService.LogAsync(ESecurityEventType.ConsentGiven, user, client, request.IpAddress, null);

        if (status == EUserClientStatus.Pending)
        {
            return Result<ConsentResponse>.Success(new ConsentResponse { Status = EUserClientStatus.Pending });
        }

        await _authWorkflow.AssignDefaultRoleIfPresentAsync(user, client);
        var response = await _authWorkflow.IssueLoginResponseAsync(user, client, payload.ResponseType, payload.RedirectUri);
        if (response.Error is not null) return Result<ConsentResponse>.Failure(response.Error);

        return Result<ConsentResponse>.Success(new ConsentResponse
        {
            AuthCode = response.AuthCode,
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            Status = EUserClientStatus.Active
        });
    }

    public async Task<Result<Unit>> RevokeConsentAsync(RevokeConsentRequest request)
    {
        var validation = await _revokeConsentValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        var client = await _authWorkflow.GetActiveClientAsync(request.ClientId);
        if (user is null || client is null) return Result<Unit>.Failure("NotFound");

        var userClient = await _userClientRepository.GetByUserAndClientAsync(user.Id, client.ClientId);
        if (userClient is null) return Result<Unit>.Failure("NotFound");

        userClient.Status = EUserClientStatus.Revoked;
        userClient.RevokedAt = DateTime.UtcNow;
        userClient.RevokedBy = user.Id.ToString();
        userClient.RevokeReason = "Consent revoked";

        await _userClientRepository.UpdateUserClientAsync(userClient);
        await _authWorkflow.RevokeRefreshTokenAsync(user, client.ClientId);
        await _securityEventService.LogAsync(ESecurityEventType.ConsentRevoked, user, client, null, null);

        return Result<Unit>.Success(Unit.Value);
    }
}
