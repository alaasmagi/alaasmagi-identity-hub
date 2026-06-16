using Application.Admin.Requests;
using FluentValidation;

namespace Application.Admin.Validators;

public sealed class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        RuleFor(request => request.Page).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 200);
        RuleFor(request => request.Search).MaximumLength(256);
    }
}

public sealed class BanUserRequestValidator : AbstractValidator<BanUserRequest>
{
    public BanUserRequestValidator()
    {
        RuleFor(request => request.TargetUserId).NotEmpty();
        RuleFor(request => request.AdminUserId).NotEmpty();
        RuleFor(request => request.Reason).NotEmpty().MaximumLength(2048);
    }
}

public sealed class UnbanUserRequestValidator : AbstractValidator<UnbanUserRequest>
{
    public UnbanUserRequestValidator()
    {
        RuleFor(request => request.TargetUserId).NotEmpty();
        RuleFor(request => request.AdminUserId).NotEmpty();
    }
}

public sealed class ApproveUserClientRequestValidator : AbstractValidator<ApproveUserClientRequest>
{
    public ApproveUserClientRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.ClientId).NotEmpty();
        RuleFor(request => request.AdminUserId).NotEmpty();
    }
}

public sealed class GetSecurityEventsRequestValidator : AbstractValidator<GetSecurityEventsRequest>
{
    public GetSecurityEventsRequestValidator()
    {
        RuleFor(request => request.Page).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 200);
    }
}
