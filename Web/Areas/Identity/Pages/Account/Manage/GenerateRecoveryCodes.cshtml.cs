using System.ComponentModel.DataAnnotations;
using Application.TwoFactor;
using Application.TwoFactor.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Identity.Pages.Account.Manage;

public class GenerateRecoveryCodesModel : PageModel
{
    private readonly ITwoFactorService _twoFactorService;

    public GenerateRecoveryCodesModel(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string[]? RecoveryCodes { get; set; }

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
            : NotFound(AccountFlow.Text(this, "Unable to load the current user."));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!AccountFlow.TryGetUserId(User, out var userId))
        {
            return NotFound(AccountFlow.Text(this, "Unable to load the current user."));
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var code = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _twoFactorService.RegenerateRecoveryCodesAsync(new RegenerateCodesRequest(userId, code));
        if (!result.IsSuccess || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(this, result.Error));
            return Page();
        }

        RecoveryCodes = result.Value.RecoveryCodes.ToArray();
        StatusMessage = AccountFlow.Text(this, "You have generated new recovery codes.");
        return RedirectToPage("./ShowRecoveryCodes");
    }
}
