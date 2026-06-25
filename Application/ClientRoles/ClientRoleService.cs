using Application.ClientRoles.Requests;
using Application.ClientRoles.Responses;
using Application.Common;
using Application.Common.Validation;
using Contracts.DataAccess;
using Domain;
using DTO.DataAccess.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.ClientRoles;

public sealed class ClientRoleService : IClientRoleService
{
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly RoleManager<AppRoleEntity> _roleManager;
    private readonly IClientRepository _clientRepository;
    private readonly IAppRoleRepository _roleRepository;
    private readonly IAppUserClientRepository _userClientRepository;
    private readonly ILookupNormalizer _normalizer;
    private readonly IValidator<SyncRolesRequest> _syncRolesValidator;
    private readonly IValidator<SetUserRolesRequest> _setUserRolesValidator;
    private readonly IValidator<RemoveUserRoleRequest> _removeUserRoleValidator;

    public ClientRoleService(
        UserManager<AppUserEntity> userManager,
        RoleManager<AppRoleEntity> roleManager,
        IClientRepository clientRepository,
        IAppRoleRepository roleRepository,
        IAppUserClientRepository userClientRepository,
        ILookupNormalizer normalizer,
        IValidator<SyncRolesRequest> syncRolesValidator,
        IValidator<SetUserRolesRequest> setUserRolesValidator,
        IValidator<RemoveUserRoleRequest> removeUserRoleValidator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _clientRepository = clientRepository;
        _roleRepository = roleRepository;
        _userClientRepository = userClientRepository;
        _normalizer = normalizer;
        _syncRolesValidator = syncRolesValidator;
        _setUserRolesValidator = setUserRolesValidator;
        _removeUserRoleValidator = removeUserRoleValidator;
    }

    public async Task<Result<SyncRolesResponse>> SyncRolesAsync(SyncRolesRequest request)
    {
        var validation = await _syncRolesValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<SyncRolesResponse>.Failure(validation.ToErrorMessage());

        var client = await _clientRepository.GetByDatabaseIdAsync(request.ClientDbId);
        if (client is null) return Result<SyncRolesResponse>.Failure("InvalidClient");

        var existingRoles = await _roleRepository.GetByClientIdAsync(client.Id);
        var rolesByNormalizedName = existingRoles
            .Where(role => !string.IsNullOrWhiteSpace(role.NormalizedName ?? role.Name))
            .ToDictionary(role => Normalize(role.Name!), role => role, StringComparer.Ordinal);

        var requestedRoles = request.Roles
            .Select(role => new RoleDefinition
            {
                Name = role.Name.Trim(),
                IsDefault = role.IsDefault
            })
            .GroupBy(role => Normalize(role.Name), StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();

        foreach (var requestedRole in requestedRoles)
        {
            var normalizedName = Normalize(requestedRole.Name);
            if (rolesByNormalizedName.ContainsKey(normalizedName))
            {
                continue;
            }

            var roleEntity = new AppRoleEntity
            {
                Name = requestedRole.Name,
                ClientId = client.Id,
                CreatedBy = client.ClientId.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = client.ClientId.ToString(),
                UpdatedAt = DateTime.UtcNow,
                ConcurrencyToken = Guid.NewGuid().ToString("N")
            };

            var createResult = await _roleManager.CreateAsync(roleEntity);
            if (!createResult.Succeeded)
            {
                return Result<SyncRolesResponse>.Failure(ToIdentityError(createResult));
            }

            rolesByNormalizedName[normalizedName] = new AppRole
            {
                Id = roleEntity.Id,
                Name = roleEntity.Name,
                NormalizedName = roleEntity.NormalizedName,
                ClientId = roleEntity.ClientId
            };
        }

        var defaultRoleDefinition = requestedRoles.FirstOrDefault(role => role.IsDefault);
        AppRole? defaultRole = null;
        if (defaultRoleDefinition is not null)
        {
            rolesByNormalizedName.TryGetValue(Normalize(defaultRoleDefinition.Name), out defaultRole);
        }

        client.DefaultRoleId = defaultRole?.Id;
        client.DefaultRole = defaultRole;
        await _clientRepository.UpdateClientAsync(client);

        return Result<SyncRolesResponse>.Success(new SyncRolesResponse
        {
            SyncedRoles = requestedRoles.Select(role => role.Name).ToArray(),
            DefaultRole = defaultRole?.Name
        });
    }

    public async Task<Result<GetUserRolesResponse>> GetUserRolesAsync(Guid clientDbId, Guid userId)
    {
        var access = await GetActiveClientUserAsync(clientDbId, userId);
        if (access.Error is not null) return Result<GetUserRolesResponse>.Failure(access.Error);

        var roles = await GetCurrentClientRoleNamesAsync(access.User!, clientDbId);
        return Result<GetUserRolesResponse>.Success(new GetUserRolesResponse
        {
            UserId = userId,
            Roles = roles.ToArray()
        });
    }

    public async Task<Result<Unit>> SetUserRolesAsync(SetUserRolesRequest request)
    {
        var validation = await _setUserRolesValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var access = await GetActiveClientUserAsync(request.ClientDbId, request.UserId);
        if (access.Error is not null) return Result<Unit>.Failure(access.Error);

        var validRoles = await _roleRepository.GetByClientIdAsync(request.ClientDbId);
        var validRoleNamesByNormalizedName = validRoles
            .Where(role => !string.IsNullOrWhiteSpace(role.Name))
            .ToDictionary(role => Normalize(role.Name!), role => role.Name!, StringComparer.Ordinal);

        var requestedRolesByNormalizedName = request.Roles
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .GroupBy(Normalize, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var requestedRole in requestedRolesByNormalizedName.Values)
        {
            if (!validRoleNamesByNormalizedName.ContainsKey(Normalize(requestedRole)))
            {
                return Result<Unit>.Failure($"InvalidRole:{requestedRole}");
            }
        }

        var currentRoles = await GetCurrentClientRoleNamesAsync(access.User!, request.ClientDbId);
        var currentByNormalizedName = currentRoles.ToDictionary(Normalize, role => role, StringComparer.Ordinal);

        var rolesToRemove = currentByNormalizedName
            .Where(current => !requestedRolesByNormalizedName.ContainsKey(current.Key))
            .Select(current => current.Value)
            .ToList();

        var rolesToAdd = requestedRolesByNormalizedName
            .Where(requested => !currentByNormalizedName.ContainsKey(requested.Key))
            .Select(requested => validRoleNamesByNormalizedName[requested.Key])
            .ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(access.User!, rolesToRemove);
            if (!removeResult.Succeeded) return Result<Unit>.Failure(ToIdentityError(removeResult));
        }

        if (rolesToAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(access.User!, rolesToAdd);
            if (!addResult.Succeeded) return Result<Unit>.Failure(ToIdentityError(addResult));
        }

        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> RemoveUserRoleAsync(RemoveUserRoleRequest request)
    {
        var validation = await _removeUserRoleValidator.ValidateAsync(request);
        if (!validation.IsValid) return Result<Unit>.Failure(validation.ToErrorMessage());

        var access = await GetActiveClientUserAsync(request.ClientDbId, request.UserId);
        if (access.Error is not null) return Result<Unit>.Failure(access.Error);

        var roleName = request.RoleName.Trim();
        var role = await _roleRepository.GetByNameAndClientIdAsync(Normalize(roleName), request.ClientDbId);
        if (role?.Name is null) return Result<Unit>.Failure($"InvalidRole:{roleName}");

        if (!await _userManager.IsInRoleAsync(access.User!, role.Name))
        {
            return Result<Unit>.Failure("RoleNotAssigned");
        }

        var removeResult = await _userManager.RemoveFromRoleAsync(access.User!, role.Name);
        return removeResult.Succeeded
            ? Result<Unit>.Success(Unit.Value)
            : Result<Unit>.Failure(ToIdentityError(removeResult));
    }

    private async Task<(AppUserEntity? User, string? Error)> GetActiveClientUserAsync(Guid clientDbId, Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (null, "UserNotInClient");

        var userClient = await _userClientRepository.GetByUserAndClientAsync(userId, clientDbId);
        if (userClient is not { Status: EUserClientStatus.Active }) return (null, "UserNotInClient");

        return (user, null);
    }

    private async Task<List<string>> GetCurrentClientRoleNamesAsync(AppUserEntity user, Guid clientDbId)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var userRolesByNormalizedName = userRoles.ToDictionary(Normalize, role => role, StringComparer.Ordinal);

        var clientRoles = await _roleRepository.GetByClientIdAsync(clientDbId);
        var clientNormalizedNames = clientRoles
            .Where(role => !string.IsNullOrWhiteSpace(role.Name))
            .Select(role => Normalize(role.Name!))
            .ToHashSet(StringComparer.Ordinal);

        return userRolesByNormalizedName
            .Where(role => clientNormalizedNames.Contains(role.Key))
            .Select(role => role.Value)
            .ToList();
    }

    private string Normalize(string roleName)
    {
        return _normalizer.NormalizeName(roleName.Trim()) ?? roleName.Trim().ToUpperInvariant();
    }

    private static string ToIdentityError(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
