using Application.Admin;
using Application.ClientRoles;
using Application.ClientRoles.Requests;
using Contracts.DataAccess;
using Domain;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Admin.Pages.Users;

public class ClientsModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly IClientRepository _clientRepository;
    private readonly IAppRoleRepository _roleRepository;
    private readonly IAppUserClientRepository _userClientRepository;
    private readonly IClientRoleService _clientRoleService;
    private readonly UserManager<AppUserEntity> _userManager;

    public ClientsModel(
        IAdminService adminService,
        IClientRepository clientRepository,
        IAppRoleRepository roleRepository,
        IAppUserClientRepository userClientRepository,
        IClientRoleService clientRoleService,
        UserManager<AppUserEntity> userManager)
    {
        _adminService = adminService;
        _clientRepository = clientRepository;
        _roleRepository = roleRepository;
        _userClientRepository = userClientRepository;
        _clientRoleService = clientRoleService;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public AppUserEntity? TargetUser { get; private set; }
    public IReadOnlyList<UserClientAccessRow> ClientAccess { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid clientId)
    {
        if (!AccountFlow.TryGetUserId(User, out var adminUserId))
        {
            return Forbid();
        }

        var client = await _clientRepository.GetByDatabaseIdAsync(clientId);
        if (client is null)
        {
            StatusMessage = "Client was not found.";
            return RedirectToPage(new { userId = UserId });
        }

        var userClient = await FindUserClientAsync(client);
        if (userClient is null)
        {
            StatusMessage = "User client access was not found.";
            return RedirectToPage(new { userId = UserId });
        }

        userClient.Status = EUserClientStatus.Active;
        userClient.GrantedAt = DateTime.UtcNow;
        userClient.GrantedBy = adminUserId.ToString();
        userClient.RevokedAt = null;
        userClient.RevokedBy = null;
        userClient.RevokeReason = null;
        await _userClientRepository.UpdateUserClientAsync(userClient);

        StatusMessage = "Client access approved.";
        return RedirectToPage(new { userId = UserId });
    }

    public async Task<IActionResult> OnPostGrantAsync(Guid clientId)
    {
        if (!AccountFlow.TryGetUserId(User, out var adminUserId))
        {
            return Forbid();
        }

        var targetUser = await _userManager.FindByIdAsync(UserId.ToString());
        var client = await _clientRepository.GetByDatabaseIdAsync(clientId);
        if (targetUser is null || client is null)
        {
            StatusMessage = "User or client was not found.";
            return RedirectToPage(new { userId = UserId });
        }

        var userClient = await FindUserClientAsync(client);
        if (userClient is null)
        {
            await _userClientRepository.AddUserClientAsync(new AppUserClient
            {
                UserId = UserId,
                ClientId = client.Id,
                Status = EUserClientStatus.Active,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = adminUserId.ToString(),
                ConsentGivenAt = DateTime.UtcNow
            });
        }
        else
        {
            userClient.Status = EUserClientStatus.Active;
            userClient.GrantedAt = DateTime.UtcNow;
            userClient.GrantedBy = adminUserId.ToString();
            userClient.RevokedAt = null;
            userClient.RevokedBy = null;
            userClient.RevokeReason = null;
            userClient.ConsentGivenAt ??= DateTime.UtcNow;
            await _userClientRepository.UpdateUserClientAsync(userClient);
        }

        StatusMessage = "Client access granted.";
        return RedirectToPage(new { userId = UserId });
    }

    public async Task<IActionResult> OnPostRevokeAsync(Guid clientId)
    {
        if (!AccountFlow.TryGetUserId(User, out var adminUserId))
        {
            return Forbid();
        }

        var client = await _clientRepository.GetByDatabaseIdAsync(clientId);
        if (client is null)
        {
            StatusMessage = "Client was not found.";
            return RedirectToPage(new { userId = UserId });
        }

        var userClient = await FindUserClientAsync(client);
        if (userClient is null)
        {
            StatusMessage = "User client access was not found.";
            return RedirectToPage(new { userId = UserId });
        }

        await RemoveClientRolesAsync(clientId);

        userClient.Status = EUserClientStatus.Revoked;
        userClient.RevokedAt = DateTime.UtcNow;
        userClient.RevokedBy = adminUserId.ToString();
        userClient.RevokeReason = "Revoked by administrator";
        await _userClientRepository.UpdateUserClientAsync(userClient);

        StatusMessage = "Client access revoked.";
        return RedirectToPage(new { userId = UserId });
    }

    public async Task<IActionResult> OnPostRolesAsync(Guid clientId, List<string>? roles)
    {
        var result = await _clientRoleService.SetUserRolesAsync(new SetUserRolesRequest
        {
            ClientDbId = clientId,
            UserId = UserId,
            Roles = roles ?? []
        });

        StatusMessage = result.IsSuccess ? "User roles updated." : result.Error;
        return RedirectToPage(new { userId = UserId });
    }

    private async Task LoadAsync()
    {
        TargetUser = await _userManager.FindByIdAsync(UserId.ToString());
        if (TargetUser is null)
        {
            ClientAccess = [];
            return;
        }

        var result = await _adminService.GetUserClientsAsync(UserId);
        var userClients = result.IsSuccess && result.Value is not null ? result.Value : [];
        var userClientsByClientId = userClients
            .GroupBy(userClient => userClient.ClientId)
            .ToDictionary(group => group.Key, group => group.First());
        var clients = await _clientRepository.GetAllClientsAsync();
        var rows = new List<UserClientAccessRow>();

        foreach (var client in clients.OrderBy(client => client.Name))
        {
            userClientsByClientId.TryGetValue(client.Id, out var userClient);
            userClient ??= userClientsByClientId.GetValueOrDefault(client.ClientId);
            var availableRoles = await _roleRepository.GetByClientIdAsync(client.Id);
            var assignedRoles = new List<string>();

            if (userClient is { Status: EUserClientStatus.Active })
            {
                var rolesResult = await _clientRoleService.GetUserRolesAsync(client.Id, UserId);
                assignedRoles = rolesResult.IsSuccess && rolesResult.Value is not null
                    ? rolesResult.Value.Roles.ToList()
                    : [];
            }

            rows.Add(new UserClientAccessRow(client, userClient, availableRoles, assignedRoles));
        }

        ClientAccess = rows;
    }

    private async Task RemoveClientRolesAsync(Guid clientId)
    {
        var user = await _userManager.FindByIdAsync(UserId.ToString());
        if (user is null)
        {
            return;
        }

        var clientRoles = await _roleRepository.GetByClientIdAsync(clientId);
        var clientRoleNames = clientRoles
            .Select(role => role.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var assignedRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = assignedRoles
            .Where(role => clientRoleNames.Contains(role))
            .ToList();

        if (rolesToRemove.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }
    }

    private async Task<AppUserClient?> FindUserClientAsync(Client client)
    {
        return await _userClientRepository.GetByUserAndClientAsync(UserId, client.Id)
            ?? await _userClientRepository.GetByUserAndClientAsync(UserId, client.ClientId);
    }

    public sealed record UserClientAccessRow(
        Client Client,
        AppUserClient? UserClient,
        IReadOnlyList<AppRole> AvailableRoles,
        IReadOnlyList<string> AssignedRoles);
}
