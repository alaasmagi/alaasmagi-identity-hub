using Application.Admin;
using Application.Admin.Requests;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Admin.Pages.Users;

public class ClientsModel : PageModel
{
    private readonly IAdminService _adminService;

    public ClientsModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    public IReadOnlyList<AppUserClient> UserClients { get; private set; } = [];

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

        await _adminService.ApproveUserClientAsync(new ApproveUserClientRequest(UserId, clientId, adminUserId));
        return RedirectToPage(new { userId = UserId });
    }

    private async Task LoadAsync()
    {
        var result = await _adminService.GetUserClientsAsync(UserId);
        UserClients = result.IsSuccess && result.Value is not null ? result.Value : [];
    }
}
