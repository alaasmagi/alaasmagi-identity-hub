using Application.TwoFactor.Requests;
using FluentValidation;

namespace Application.TwoFactor.Validators;

public sealed class EnableTwoFactorRequestValidator : AbstractValidator<EnableTwoFactorRequest>
{
    public EnableTwoFactorRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Code).NotEmpty();
    }
}

public sealed class DisableTwoFactorRequestValidator : AbstractValidator<DisableTwoFactorRequest>
{
    public DisableTwoFactorRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Code).NotEmpty();
    }
}

public sealed class RegenerateCodesRequestValidator : AbstractValidator<RegenerateCodesRequest>
{
    public RegenerateCodesRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Code).NotEmpty();
    }
}

public sealed class TwoFactorLoginRequestValidator : AbstractValidator<TwoFactorLoginRequest>
{
    public TwoFactorLoginRequestValidator()
    {
        RuleFor(request => request.TempToken).NotEmpty();
        RuleFor(request => request.Code).NotEmpty();
    }
}

public sealed class RecoveryLoginRequestValidator : AbstractValidator<RecoveryLoginRequest>
{
    public RecoveryLoginRequestValidator()
    {
        RuleFor(request => request.TempToken).NotEmpty();
        RuleFor(request => request.RecoveryCode).NotEmpty();
    }
}
