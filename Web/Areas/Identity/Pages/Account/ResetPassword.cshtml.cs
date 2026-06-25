using System.ComponentModel.DataAnnotations;
using Application.Auth;
using Application.Auth.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ResetPasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Token { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;
    }

    public IActionResult OnGet(Guid userId, string? token = null, string? code = null)
    {
        var resetToken = token ?? code;
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(resetToken))
        {
            return BadRequest(AccountFlow.Text(this, "A user id and token must be supplied for password reset."));
        }

        Input = new InputModel
        {
            UserId = userId,
            Token = resetToken
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.ResetPasswordAsync(
            new ResetPasswordRequest(Input.UserId, Input.Token, Input.Password));

        if (result.IsSuccess)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(this, result.Error));
        return Page();
    }
}
