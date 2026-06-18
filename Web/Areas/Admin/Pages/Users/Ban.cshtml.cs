using System.ComponentModel.DataAnnotations;
using Application.Admin;
using Application.Admin.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Admin.Pages.Users;

public class BanModel : PageModel
{
    private readonly IAdminService _adminService;

    public BanModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IsBanned { get; set; }

    [BindProperty]
    [Required]
    public string Reason { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!AccountFlow.TryGetUserId(User, out var adminUserId))
        {
            return Forbid();
        }

        if (IsBanned)
        {
            await _adminService.UnbanUserAsync(new UnbanUserRequest(UserId, adminUserId));
            return RedirectToPage("./Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _adminService.BanUserAsync(new BanUserRequest(UserId, adminUserId, Reason));
        return RedirectToPage("./Index");
    }
}
