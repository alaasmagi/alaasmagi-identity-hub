using System.Text.Encodings.Web;
using Application.Auth.Responses;
using Application.Common;
using Application.Common.Abstractions;
using Application.Common.Auth;
using Application.Common.Validation;
using Application.TwoFactor.Requests;
using Application.TwoFactor.Responses;
using Domain;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.TwoFactor;

public sealed class TwoFactorService : ITwoFactorService
{
    private const int RecoveryCodeCount = 10;

    private readonly UserManager<AppUser> _userManager;
    private readonly ISecurityEventService _securityEventService;
    private readonly AuthWorkflow _authWorkflow;
    private readonly IValidator<EnableTwoFactorRequest> _enableValidator;
    private readonly IValidator<DisableTwoFactorRequest> _disableValidator;
    private readonly IValidator<RegenerateCodesRequest> _regenerateValidator;
    private readonly IValidator<TwoFactorLoginRequest> _twoFactorLoginValidator;
    private readonly IValidator<RecoveryLoginRequest> _recoveryLoginValidator;

    public TwoFactorService(
        UserManager<AppUser> userManager,
        ISecurityEventService securityEventService,
        AuthWorkflow authWorkflow,
        IValidator<EnableTwoFactorRequest> enableValidator,
        IValidator<DisableTwoFactorRequest> disableValidator,
        IValidator<RegenerateCodesRequest> regenerateValidator,
        IValidator<TwoFactorLoginRequest> twoFactorLoginValidator,
        IValidator<RecoveryLoginRequest> recoveryLoginValidator)
    {
        _userManager = userManager;
        _securityEventService = securityEventService;
        _authWorkflow = authWorkflow;
        _enableValidator = enableValidator;
        _disableValidator = disableValidator;
        _regenerateValidator = regenerateValidator;
        _twoFactorLoginValidator = twoFactorLoginValidator;
        _recoveryLoginValidator = recoveryLoginValidator;
    }

    public async Task<Result<TwoFactorStatusResponse>> GetStatusAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result<TwoFactorStatusResponse>.Failure("UserNotFound");

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        return Result<TwoFactorStatusResponse>.Success(new TwoFactorStatusResponse(
            await _userManager.GetTwoFactorEnabledAsync(user),
            !string.IsNullOrWhiteSpace(key),
            await _userManager.CountRecoveryCodesAsync(user)));
    }

    public async Task<Result<AuthenticatorSetupResponse>> SetupAuthenticatorAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result<AuthenticatorSetupResponse>.Failure("UserNotFound");

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var email = await _userManager.GetEmailAsync(user) ?? user.UserName ?? user.Id.ToString();
        var uri = GenerateQrCodeUri(email, key ?? string.Empty);
        return Result<AuthenticatorSetupResponse>.Success(new AuthenticatorSetupResponse(key ?? string.Empty, uri));
    }

    public async Task<Result<RecoveryCodesResponse>> EnableTwoFactorAsync(EnableTwoFactorRequest request)
    {
        var validation = await _enableValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<RecoveryCodesResponse>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return Result<RecoveryCodesResponse>.Failure("UserNotFound");

        if (!await VerifyAuthenticatorCodeAsync(user, request.Code))
        {
            return Result<RecoveryCodesResponse>.Failure("InvalidCode");
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodeCount);
        await _securityEventService.LogAsync(ESecurityEventType.TwoFactorEnabled, user, null, null, null);

        return Result<RecoveryCodesResponse>.Success(new RecoveryCodesResponse(codes?.ToList() ?? []));
    }

    public async Task<Result<Unit>> DisableTwoFactorAsync(DisableTwoFactorRequest request)
    {
        var validation = await _disableValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return Result<Unit>.Failure("UserNotFound");

        if (!await VerifyAuthenticatorCodeAsync(user, request.Code))
        {
            return Result<Unit>.Failure("InvalidCode");
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _securityEventService.LogAsync(ESecurityEventType.TwoFactorDisabled, user, null, null, null);

        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<RecoveryCodesResponse>> RegenerateRecoveryCodesAsync(RegenerateCodesRequest request)
    {
        var validation = await _regenerateValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<RecoveryCodesResponse>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return Result<RecoveryCodesResponse>.Failure("UserNotFound");

        if (!await VerifyAuthenticatorCodeAsync(user, request.Code))
        {
            return Result<RecoveryCodesResponse>.Failure("InvalidCode");
        }

        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodeCount);
        return Result<RecoveryCodesResponse>.Success(new RecoveryCodesResponse(codes?.ToList() ?? []));
    }

    public async Task<Result<LoginResponse>> LoginWithTwoFactorAsync(TwoFactorLoginRequest request)
    {
        var validation = await _twoFactorLoginValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<LoginResponse>.Failure(validation.ToErrorMessage());

        var context = await GetTempLoginContextAsync(request.TempToken);
        if (context is null) return Result<LoginResponse>.Failure("InvalidTempToken");
        var loginContext = context.Value;

        if (!await VerifyAuthenticatorCodeAsync(loginContext.User, request.Code))
        {
            return Result<LoginResponse>.Failure("InvalidCode");
        }

        await _authWorkflow.ClearTempTokenAsync(loginContext.User, loginContext.Client.ClientId);
        var response = await _authWorkflow.ContinueAfterAuthenticationAsync(loginContext.User, loginContext.Client, loginContext.ResponseType);
        return response.Error is null
            ? Result<LoginResponse>.Success(response)
            : Result<LoginResponse>.Failure(response.Error);
    }

    public async Task<Result<LoginResponse>> LoginWithRecoveryCodeAsync(RecoveryLoginRequest request)
    {
        var validation = await _recoveryLoginValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<LoginResponse>.Failure(validation.ToErrorMessage());

        var context = await GetTempLoginContextAsync(request.TempToken);
        if (context is null) return Result<LoginResponse>.Failure("InvalidTempToken");
        var loginContext = context.Value;

        var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(loginContext.User, request.RecoveryCode);
        if (!result.Succeeded) return Result<LoginResponse>.Failure("InvalidRecoveryCode");

        await _authWorkflow.ClearTempTokenAsync(loginContext.User, loginContext.Client.ClientId);
        var response = await _authWorkflow.ContinueAfterAuthenticationAsync(loginContext.User, loginContext.Client, loginContext.ResponseType);
        return response.Error is null
            ? Result<LoginResponse>.Success(response)
            : Result<LoginResponse>.Failure(response.Error);
    }

    private Task<bool> VerifyAuthenticatorCodeAsync(AppUser user, string code)
    {
        var normalized = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        return _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, normalized);
    }

    private async Task<(AppUser User, Domain.Client Client, string ResponseType)?> GetTempLoginContextAsync(string tempToken)
    {
        var payload = await _authWorkflow.ValidateTempTokenAsync(tempToken);
        if (payload is null) return null;

        var user = await _userManager.FindByIdAsync(payload.UserId.ToString());
        var client = await _authWorkflow.GetActiveClientAsync(payload.ClientId);
        return user is null || client is null ? null : (user, client, payload.ResponseType);
    }

    private static string GenerateQrCodeUri(string email, string key)
    {
        return "otpauth://totp/"
            + UrlEncoder.Default.Encode("IdentityHub:" + email)
            + "?secret="
            + UrlEncoder.Default.Encode(key)
            + "&issuer="
            + UrlEncoder.Default.Encode("IdentityHub")
            + "&digits=6";
    }
}
