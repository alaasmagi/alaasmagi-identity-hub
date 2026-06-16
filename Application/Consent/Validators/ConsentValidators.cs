using Application.Consent.Requests;
using FluentValidation;

namespace Application.Consent.Validators;

public sealed class GrantConsentRequestValidator : AbstractValidator<GrantConsentRequest>
{
    public GrantConsentRequestValidator()
    {
        RuleFor(request => request.ConsentToken).NotEmpty();
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.IpAddress).MaximumLength(45);
    }
}

public sealed class RevokeConsentRequestValidator : AbstractValidator<RevokeConsentRequest>
{
    public RevokeConsentRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.ClientId).NotEmpty();
    }
}
