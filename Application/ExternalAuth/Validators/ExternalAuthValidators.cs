using Application.ExternalAuth.Requests;
using FluentValidation;

namespace Application.ExternalAuth.Validators;

public sealed class ExternalCallbackRequestValidator : AbstractValidator<ExternalCallbackRequest>
{
    public ExternalCallbackRequestValidator()
    {
        RuleFor(request => request.Provider).NotEmpty();
        RuleFor(request => request.ClientId).NotEmpty();
        RuleFor(request => request.RedirectUri).NotEmpty().Must(value => Uri.TryCreate(value, UriKind.Absolute, out _));
        RuleFor(request => request.TenantId).MaximumLength(128);
    }
}

public sealed class ExchangeCodeRequestValidator : AbstractValidator<ExchangeCodeRequest>
{
    public ExchangeCodeRequestValidator()
    {
        RuleFor(request => request.Code).NotEmpty();
        RuleFor(request => request.ClientId).NotEmpty();
        RuleFor(request => request.ClientSecret).NotEmpty();
    }
}
