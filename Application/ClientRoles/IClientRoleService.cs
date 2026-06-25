using Application.ClientRoles.Requests;
using Application.ClientRoles.Responses;
using Application.Common;

namespace Application.ClientRoles;

public interface IClientRoleService
{
    Task<Result<SyncRolesResponse>> SyncRolesAsync(SyncRolesRequest request);
    Task<Result<GetUserRolesResponse>> GetUserRolesAsync(Guid clientDbId, Guid userId);
    Task<Result<Unit>> SetUserRolesAsync(SetUserRolesRequest request);
    Task<Result<Unit>> RemoveUserRoleAsync(RemoveUserRoleRequest request);
}
