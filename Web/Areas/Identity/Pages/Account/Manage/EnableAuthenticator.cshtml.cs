using System.ComponentModel.DataAnnotations;
using Application.TwoFactor;
using Application.TwoFactor.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Identity.Pages.Account.Manage;

public class EnableAuthenticatorModel : PageModel
{
    private readonly ITwoFactorService _twoFactorService;

    public EnableAuthenticatorModel(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    public string SharedKey { get; set; } = default!;
    public string AuthenticatorUri { get; set; } = default!;

    [TempData]
    public string[]? RecoveryCodes { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string Code { get; set; } = default!;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return await LoadSetupAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!AccountFlow.TryGetUserId(User, out var userId))
        {
            return NotFound(AccountFlow.Text(this, "Unable to load the current user."));
        }

        if (!ModelState.IsValid)
        {
            await LoadSetupAsync();
            return Page();
        }

        var code = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _twoFactorService.EnableTwoFactorAsync(new EnableTwoFactorRequest(userId, code));
        if (!result.IsSuccess || result.Value is null)
        {
            ModelState.AddModelError("Input.Code", AccountFlow.ToDisplayError(this, result.Error));
            await LoadSetupAsync();
            return Page();
        }

        RecoveryCodes = result.Value.RecoveryCodes.ToArray();
        StatusMessage = AccountFlow.Text(this, "Your authenticator app has been verified.");
        return RedirectToPage("./ShowRecoveryCodes");
    }

    private async Task<IActionResult> LoadSetupAsync()
    {
        if (!AccountFlow.TryGetUserId(User, out var userId))
        {
            return NotFound(AccountFlow.Text(this, "Unable to load the current user."));
        }

        var result = await _twoFactorService.SetupAuthenticatorAsync(userId);
        if (!result.IsSuccess || result.Value is null)
        {
            return NotFound(AccountFlow.Text(this, "Unable to load authenticator setup."));
        }

        SharedKey = result.Value.SharedKey;
        AuthenticatorUri = result.Value.QrCodeUri;
        return Page();
    }
}
