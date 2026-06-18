using System.ComponentModel.DataAnnotations;
using Application.TwoFactor;
using Application.TwoFactor.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Identity.Pages.Account.Manage;

public class Disable2faModel : PageModel
{
    private readonly ITwoFactorService _twoFactorService;

    public Disable2faModel(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public sealed class InputModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string Code { get; set; } = default!;
    }

    public IActionResult OnGet()
    {
        return AccountFlow.TryGetUserId(User, out _)
            ? Page()
            : NotFound("Unable to load the current user.");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!AccountFlow.TryGetUserId(User, out var userId))
        {
            return NotFound("Unable to load the current user.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var code = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _twoFactorService.DisableTwoFactorAsync(new DisableTwoFactorRequest(userId, code));
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(result.Error));
            return Page();
        }

        StatusMessage = "2FA has been disabled. You can reenable 2FA when you set up an authenticator app.";
        return RedirectToPage("./TwoFactorAuthentication");
    }
}
