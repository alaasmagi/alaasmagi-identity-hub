using System.ComponentModel.DataAnnotations;
using Application.TwoFactor;
using Application.TwoFactor.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

public class LoginWith2faModel : PageModel
{
    private readonly ITwoFactorService _twoFactorService;

    public LoginWith2faModel(ITwoFactorService twoFactorService)
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
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; } = default!;

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }
    }

    public IActionResult OnGet()
    {
        if (string.IsNullOrWhiteSpace(TempToken))
        {
            ModelState.AddModelError(string.Empty, AccountFlow.Text(this, "The two-factor login request is no longer valid."));
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var code = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _twoFactorService.LoginWithTwoFactorAsync(new TwoFactorLoginRequest(TempToken ?? string.Empty, code));

        return AccountFlow.HandleLoginResult(this, result, ClientId, RedirectUri);
    }
}
