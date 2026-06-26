using Application.Admin;
using Application.Admin.Requests;
using Application.Admin.Responses;
using Application.Common;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Admin.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly UserManager<AppUserEntity> _userManager;

    public IndexModel(
        IAdminService adminService,
        UserManager<AppUserEntity> userManager)
    {
        _adminService = adminService;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResponse<UserSummary>? Users { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var result = await _adminService.GetUsersAsync(new GetUsersRequest(Math.Max(1, PageNumber), 25, Search));
        Users = result.IsSuccess ? result.Value : null;
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid userId)
    {
        if (!AccountFlow.TryGetUserId(User, out var adminUserId))
        {
            return Forbid();
        }

        if (userId == adminUserId)
        {
            StatusMessage = "You cannot delete your own user account.";
            return RedirectToPage(new { Search, PageNumber });
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            StatusMessage = "User was not found.";
            return RedirectToPage(new { Search, PageNumber });
        }

        var result = await _userManager.DeleteAsync(user);
        StatusMessage = result.Succeeded
            ? "User deleted from database."
            : string.Join(" ", result.Errors.Select(error => error.Description));

        return RedirectToPage(new { Search, PageNumber });
    }
}
