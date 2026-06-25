using Application.ClientRoles.Requests;
using FluentValidation;

namespace Application.ClientRoles.Validators;

public sealed class SyncRolesRequestValidator : AbstractValidator<SyncRolesRequest>
{
    public SyncRolesRequestValidator()
    {
        RuleFor(request => request.ClientDbId).NotEmpty();
        RuleFor(request => request.Roles).NotNull().NotEmpty();
        RuleForEach(request => request.Roles).ChildRules(role =>
        {
            role.RuleFor(definition => definition.Name).NotEmpty();
        });
        RuleFor(request => request.Roles)
            .Must(roles => roles.Count(role => role.IsDefault) <= 1)
            .WithMessage("Only one role can be marked as default.");
    }
}
