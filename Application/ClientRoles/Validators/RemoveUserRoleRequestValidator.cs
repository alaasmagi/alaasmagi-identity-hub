using Application.ClientRoles.Requests;
using FluentValidation;

namespace Application.ClientRoles.Validators;

public sealed class RemoveUserRoleRequestValidator : AbstractValidator<RemoveUserRoleRequest>
{
    public RemoveUserRoleRequestValidator()
    {
        RuleFor(request => request.ClientDbId).NotEmpty();
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.RoleName).NotEmpty();
    }
}
