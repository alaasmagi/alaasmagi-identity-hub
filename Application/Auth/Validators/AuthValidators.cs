using Application.Auth.Requests;
using FluentValidation;

namespace Application.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Password).NotEmpty().MinimumLength(6);
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(256);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Password).NotEmpty();
        RuleFor(request => request.ClientId).NotEmpty();
        RuleFor(request => request.ResponseType).Must(value => value is "jwt" or "cookie");
    }
}

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken).NotEmpty();
        RuleFor(request => request.ClientId).NotEmpty();
    }
}

public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.RefreshToken).NotEmpty();
        RuleFor(request => request.UserId).NotEmpty();
    }
}

public sealed class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Token).NotEmpty();
    }
}

public sealed class PasswordResetRequestValidator : AbstractValidator<PasswordResetRequest>
{
    public PasswordResetRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Token).NotEmpty();
        RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(6);
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.CurrentPassword).NotEmpty();
        RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(6);
    }
}
