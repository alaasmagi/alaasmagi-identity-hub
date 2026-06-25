using Application.ClientRoles.Requests;
using FluentValidation;

namespace Application.ClientRoles.Validators;

public sealed class SetUserRolesRequestValidator : AbstractValidator<SetUserRolesRequest>
{
    public SetUserRolesRequestValidator()
    {
        RuleFor(request => request.ClientDbId).NotEmpty();
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Roles).NotNull();
        RuleForEach(request => request.Roles).NotEmpty();
    }
}
