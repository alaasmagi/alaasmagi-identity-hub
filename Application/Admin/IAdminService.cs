using Application.Admin.Requests;
using Application.Admin.Responses;
using Application.Common;
using Domain;

namespace Application.Admin;

public interface IAdminService
{
    Task<Result<PagedResponse<UserSummary>>> GetUsersAsync(GetUsersRequest request);
    Task<Result<Unit>> BanUserAsync(BanUserRequest request);
    Task<Result<Unit>> UnbanUserAsync(UnbanUserRequest request);
    Task<Result<List<AppUserClient>>> GetUserClientsAsync(Guid userId);
    Task<Result<Unit>> ApproveUserClientAsync(ApproveUserClientRequest request);
    Task<Result<PagedResponse<SecurityEvent>>> GetSecurityEventsAsync(GetSecurityEventsRequest request);
}
