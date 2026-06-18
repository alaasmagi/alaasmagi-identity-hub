using Application.Auth.Requests;
using Application.Auth.Responses;
using Application.Common;
using Application.Common.Abstractions;
using Application.Common.Auth;
using Application.Common.Validation;
using Domain;
using DTO.DataAccess.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly SignInManager<AppUserEntity> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ISecurityEventService _securityEventService;
    private readonly AuthWorkflow _authWorkflow;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;
    private readonly IValidator<LogoutRequest> _logoutValidator;
    private readonly IValidator<ConfirmEmailRequest> _confirmEmailValidator;
    private readonly IValidator<PasswordResetRequest> _passwordResetValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public AuthService(
        UserManager<AppUserEntity> userManager,
        SignInManager<AppUserEntity> signInManager,
        IEmailService emailService,
        ISecurityEventService securityEventService,
        AuthWorkflow authWorkflow,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator,
        IValidator<LogoutRequest> logoutValidator,
        IValidator<ConfirmEmailRequest> confirmEmailValidator,
        IValidator<PasswordResetRequest> passwordResetValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _securityEventService = securityEventService;
        _authWorkflow = authWorkflow;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _logoutValidator = logoutValidator;
        _confirmEmailValidator = confirmEmailValidator;
        _passwordResetValidator = passwordResetValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<RegisterResponse>.Failure(validation.ToErrorMessage());

        var user = new AppUserEntity
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            IsActive = true,
            CreatedBy = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = request.Email,
            UpdatedAt = DateTime.UtcNow,
            ConcurrencyToken = Guid.NewGuid().ToString("N")
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Result<RegisterResponse>.Failure("RegistrationFailed");
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendEmailConfirmationAsync(user.ToDomainUser(), token);
        await _securityEventService.LogAsync(ESecurityEventType.Login, user.ToDomainUser(), null, null, null);

        return Result<RegisterResponse>.Success(new RegisterResponse(user.Id));
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<LoginResponse>.Failure(validation.ToErrorMessage());

        var client = await _authWorkflow.GetActiveClientAsync(request.ClientId);
        if (client is null) return Result<LoginResponse>.Failure("InvalidClient");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result<LoginResponse>.Failure("InvalidCredentials");
        }

        if (!user.IsActive || user.IsBanned)
        {
            await _securityEventService.LogAsync(ESecurityEventType.FailedAttempt, user.ToDomainUser(), client, null, null);
            return Result<LoginResponse>.Failure("InvalidCredentials");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (signInResult.IsNotAllowed)
        {
            return Result<LoginResponse>.Failure("EmailNotConfirmed");
        }

        if (!signInResult.Succeeded)
        {
            await _securityEventService.LogAsync(ESecurityEventType.FailedAttempt, user.ToDomainUser(), client, null, null);
            return Result<LoginResponse>.Failure("InvalidCredentials");
        }

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            var tempToken = await _authWorkflow.CreateTempTokenAsync(user, client, request.ResponseType, request.RedirectUri);
            return Result<LoginResponse>.Success(new LoginResponse
            {
                RequiresTwoFactor = true,
                TempToken = tempToken
            });
        }

        if (request.ResponseType == "cookie")
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        var response = await _authWorkflow.ContinueAfterAuthenticationAsync(user, client, request.ResponseType, request.RedirectUri);
        if (response.Error is not null) return Result<LoginResponse>.Failure(response.Error);

        await _securityEventService.LogAsync(ESecurityEventType.Login, user.ToDomainUser(), client, null, null);
        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var validation = await _refreshTokenValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<TokenResponse>.Failure(validation.ToErrorMessage());

        var payload = TokenPayloads.Unprotect<RefreshTokenPayload>(request.RefreshToken);
        if (payload is null || payload.ClientId != request.ClientId)
        {
            return Result<TokenResponse>.Failure("InvalidRefreshToken");
        }

        var user = await _userManager.FindByIdAsync(payload.UserId.ToString());
        var client = await _authWorkflow.GetActiveClientAsync(request.ClientId);
        if (user is null || client is null)
        {
            return Result<TokenResponse>.Failure("InvalidRefreshToken");
        }

        if (!await _authWorkflow.ValidateRefreshTokenAsync(user, request.ClientId, request.RefreshToken))
        {
            return Result<TokenResponse>.Failure("InvalidRefreshToken");
        }

        var response = await _authWorkflow.ContinueAfterAuthenticationAsync(user, client, "jwt");
        if (response.Error is not null || response.AccessToken is null || response.RefreshToken is null)
        {
            return Result<TokenResponse>.Failure(response.Error ?? "AccessNotActive");
        }

        await _securityEventService.LogAsync(ESecurityEventType.TokenRefresh, user.ToDomainUser(), client, null, null);
        return Result<TokenResponse>.Success(new TokenResponse(response.AccessToken, response.RefreshToken));
    }

    public async Task<Result<Unit>> LogoutAsync(LogoutRequest request)
    {
        var validation = await _logoutValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var payload = TokenPayloads.Unprotect<RefreshTokenPayload>(request.RefreshToken);
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (payload is null || user is null || payload.UserId != request.UserId)
        {
            return Result<Unit>.Failure("InvalidRefreshToken");
        }

        await _authWorkflow.RevokeRefreshTokenAsync(user, payload.ClientId);
        var client = await _authWorkflow.GetActiveClientAsync(payload.ClientId);
        await _securityEventService.LogAsync(ESecurityEventType.Logout, user.ToDomainUser(), client, null, null);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var validation = await _confirmEmailValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return Result<Unit>.Failure("InvalidToken");

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        return result.Succeeded ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("InvalidToken");
    }

    public async Task<Result<Unit>> RequestPasswordResetAsync(PasswordResetRequest request)
    {
        var validation = await _passwordResetValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetAsync(user.ToDomainUser(), token);
        }

        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var validation = await _resetPasswordValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return Result<Unit>.Failure("InvalidToken");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("InvalidToken");
    }

    public async Task<Result<Unit>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var validation = await _changePasswordValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return Result<Unit>.Failure("UserNotFound");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded) return Result<Unit>.Failure("PasswordChangeFailed");

        await _securityEventService.LogAsync(ESecurityEventType.PasswordChanged, user.ToDomainUser(), null, null, null);
        return Result<Unit>.Success(Unit.Value);
    }
}
