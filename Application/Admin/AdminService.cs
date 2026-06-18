using Application.Admin.Requests;
using Application.Admin.Responses;
using Application.Common;
using Application.Common.Abstractions;
using Application.Common.Auth;
using Application.Common.Validation;
using Contracts.DataAccess;
using Domain;
using DTO.DataAccess.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin;

public sealed class AdminService : IAdminService
{
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly IAppUserClientRepository _userClientRepository;
    private readonly ISecurityEventRepository _securityEventRepository;
    private readonly ISecurityEventService _securityEventService;
    private readonly AuthWorkflow _authWorkflow;
    private readonly IValidator<GetUsersRequest> _getUsersValidator;
    private readonly IValidator<BanUserRequest> _banUserValidator;
    private readonly IValidator<UnbanUserRequest> _unbanUserValidator;
    private readonly IValidator<ApproveUserClientRequest> _approveUserClientValidator;
    private readonly IValidator<GetSecurityEventsRequest> _getSecurityEventsValidator;

    public AdminService(
        UserManager<AppUserEntity> userManager,
        IAppUserClientRepository userClientRepository,
        ISecurityEventRepository securityEventRepository,
        ISecurityEventService securityEventService,
        AuthWorkflow authWorkflow,
        IValidator<GetUsersRequest> getUsersValidator,
        IValidator<BanUserRequest> banUserValidator,
        IValidator<UnbanUserRequest> unbanUserValidator,
        IValidator<ApproveUserClientRequest> approveUserClientValidator,
        IValidator<GetSecurityEventsRequest> getSecurityEventsValidator)
    {
        _userManager = userManager;
        _userClientRepository = userClientRepository;
        _securityEventRepository = securityEventRepository;
        _securityEventService = securityEventService;
        _authWorkflow = authWorkflow;
        _getUsersValidator = getUsersValidator;
        _banUserValidator = banUserValidator;
        _unbanUserValidator = unbanUserValidator;
        _approveUserClientValidator = approveUserClientValidator;
        _getSecurityEventsValidator = getSecurityEventsValidator;
    }

    public async Task<Result<PagedResponse<UserSummary>>> GetUsersAsync(GetUsersRequest request)
    {
        var validation = await _getUsersValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<PagedResponse<UserSummary>>.Failure(validation.ToErrorMessage());

        var query = _userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(user =>
                (user.Email != null && user.Email.Contains(search)) ||
                user.FullName.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(user => user.Email)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(user => new UserSummary(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                user.IsActive,
                user.IsBanned,
                user.BanReason))
            .ToListAsync();

        return Result<PagedResponse<UserSummary>>.Success(new PagedResponse<UserSummary>(
            users,
            request.Page,
            request.PageSize,
            totalCount));
    }

    public async Task<Result<Unit>> BanUserAsync(BanUserRequest request)
    {
        var validation = await _banUserValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.TargetUserId.ToString());
        var admin = await _userManager.FindByIdAsync(request.AdminUserId.ToString());
        if (user is null || admin is null) return Result<Unit>.Failure("UserNotFound");

        user.IsBanned = true;
        user.IsActive = false;
        user.BanReason = request.Reason;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return Result<Unit>.Failure("UserUpdateFailed");

        await _authWorkflow.RevokeAllRefreshTokensAsync(user);
        await _securityEventService.LogAsync(ESecurityEventType.AccountBanned, user.ToDomainUser(), null, null, null);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> UnbanUserAsync(UnbanUserRequest request)
    {
        var validation = await _unbanUserValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var user = await _userManager.FindByIdAsync(request.TargetUserId.ToString());
        var admin = await _userManager.FindByIdAsync(request.AdminUserId.ToString());
        if (user is null || admin is null) return Result<Unit>.Failure("UserNotFound");

        user.IsBanned = false;
        user.IsActive = true;
        user.BanReason = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return Result<Unit>.Failure("UserUpdateFailed");

        await _securityEventService.LogAsync(ESecurityEventType.AccountUnbanned, user.ToDomainUser(), null, null, null);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<List<AppUserClient>>> GetUserClientsAsync(Guid userId)
    {
        if (userId == Guid.Empty) return Result<List<AppUserClient>>.Failure("InvalidUserId");

        var userClients = await _userClientRepository.GetByUserIdAsync(userId);
        return Result<List<AppUserClient>>.Success(userClients);
    }

    public async Task<Result<Unit>> ApproveUserClientAsync(ApproveUserClientRequest request)
    {
        var validation = await _approveUserClientValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var userClient = await _userClientRepository.GetByUserAndClientAsync(request.UserId, request.ClientId);
        if (userClient is null) return Result<Unit>.Failure("UserClientNotFound");

        userClient.Status = EUserClientStatus.Active;
        userClient.GrantedAt = DateTime.UtcNow;
        userClient.GrantedBy = request.AdminUserId.ToString();
        await _userClientRepository.UpdateUserClientAsync(userClient);

        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<PagedResponse<SecurityEvent>>> GetSecurityEventsAsync(GetSecurityEventsRequest request)
    {
        var validation = await _getSecurityEventsValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<PagedResponse<SecurityEvent>>.Failure(validation.ToErrorMessage());

        var (items, totalCount) = await _securityEventRepository.GetPagedAsync(
            request.UserId,
            request.ClientId,
            request.Page,
            request.PageSize);

        return Result<PagedResponse<SecurityEvent>>.Success(new PagedResponse<SecurityEvent>(
            items,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
