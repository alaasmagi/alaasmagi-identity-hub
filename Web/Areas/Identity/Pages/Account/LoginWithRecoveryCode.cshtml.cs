using System.ComponentModel.DataAnnotations;
using Application.TwoFactor;
using Application.TwoFactor.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

public class LoginWithRecoveryCodeModel : PageModel
{
    private readonly ITwoFactorService _twoFactorService;

    public LoginWithRecoveryCodeModel(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid ClientId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RedirectUri { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TempToken { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public sealed class InputModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; } = default!;
    }

    public IActionResult OnGet()
    {
        if (string.IsNullOrWhiteSpace(TempToken))
        {
            ModelState.AddModelError(string.Empty, "The two-factor login request is no longer valid.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);
        var result = await _twoFactorService.LoginWithRecoveryCodeAsync(
            new RecoveryLoginRequest(TempToken ?? string.Empty, recoveryCode));

        return AccountFlow.HandleLoginResult(this, result, ClientId, RedirectUri);
    }
}
