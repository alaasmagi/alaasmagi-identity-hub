using Application.TwoFactor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Identity.Pages.Account.Manage;

public class TwoFactorAuthenticationModel : PageModel
{
    private readonly ITwoFactorService _twoFactorService;

    public TwoFactorAuthenticationModel(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }

    [BindProperty]
    public bool Is2faEnabled { get; set; }

    public bool IsMachineRemembered { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!AccountFlow.TryGetUserId(User, out var userId))
        {
            return NotFound(AccountFlow.Text(this, "Unable to load the current user."));
        }

        var result = await _twoFactorService.GetStatusAsync(userId);
        if (!result.IsSuccess || result.Value is null)
        {
            return NotFound(AccountFlow.Text(this, "Unable to load two-factor status."));
        }

        HasAuthenticator = result.Value.HasAuthenticator;
        Is2faEnabled = result.Value.IsEnabled;
        RecoveryCodesLeft = result.Value.RecoveryCodesLeft;
        IsMachineRemembered = false;

        return Page();
    }
}
