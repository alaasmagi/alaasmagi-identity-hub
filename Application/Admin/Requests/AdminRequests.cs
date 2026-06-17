namespace Application.Admin.Requests;

/// <summary>
/// Request to list users.
/// </summary>
/// <param name="Page">The requested page number.</param>
/// <param name="PageSize">The requested page size.</param>
/// <param name="Search">The optional search text.</param>
public sealed record GetUsersRequest(int Page, int PageSize, string? Search);

/// <summary>
/// Request to ban a user.
/// </summary>
/// <param name="TargetUserId">The user identifier to ban.</param>
/// <param name="AdminUserId">The acting admin user identifier.</param>
/// <param name="Reason">The ban reason.</param>
public sealed record BanUserRequest(Guid TargetUserId, Guid AdminUserId, string Reason);

/// <summary>
/// Request to unban a user.
/// </summary>
/// <param name="TargetUserId">The user identifier to unban.</param>
/// <param name="AdminUserId">The acting admin user identifier.</param>
public sealed record UnbanUserRequest(Guid TargetUserId, Guid AdminUserId);

/// <summary>
/// Request to approve a user-client relationship.
/// </summary>
/// <param name="UserId">The target user identifier.</param>
/// <param name="ClientId">The target client identifier.</param>
/// <param name="AdminUserId">The acting admin user identifier.</param>
public sealed record ApproveUserClientRequest(Guid UserId, Guid ClientId, Guid AdminUserId);

/// <summary>
/// Request to list security events.
/// </summary>
/// <param name="UserId">The optional user filter.</param>
/// <param name="ClientId">The optional client filter.</param>
/// <param name="Page">The requested page number.</param>
/// <param name="PageSize">The requested page size.</param>
public sealed record GetSecurityEventsRequest(Guid? UserId, Guid? ClientId, int Page, int PageSize);
