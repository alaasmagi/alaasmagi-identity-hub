namespace Application.Admin.Requests;

public sealed record GetUsersRequest(int Page, int PageSize, string? Search);
public sealed record BanUserRequest(Guid TargetUserId, Guid AdminUserId, string Reason);
public sealed record UnbanUserRequest(Guid TargetUserId, Guid AdminUserId);
public sealed record ApproveUserClientRequest(Guid UserId, Guid ClientId, Guid AdminUserId);
public sealed record GetSecurityEventsRequest(Guid? UserId, Guid? ClientId, int Page, int PageSize);
