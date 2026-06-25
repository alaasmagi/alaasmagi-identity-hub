using System.ComponentModel.DataAnnotations;
using Application.Auth;
using Application.Auth.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Areas.Identity.Pages.Account;

namespace Web.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ChangePasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public sealed class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;
    }

    public IActionResult OnGet()
    {
        return AccountFlow.TryGetUserId(User, out _)
            ? Page()
            : NotFound(AccountFlow.Text(this, "Unable to load the current user."));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!AccountFlow.TryGetUserId(User, out var userId))
        {
            return NotFound(AccountFlow.Text(this, "Unable to load the current user."));
        }

        var result = await _authService.ChangePasswordAsync(
            new ChangePasswordRequest(userId, Input.OldPassword, Input.NewPassword));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(this, result.Error));
            return Page();
        }

        StatusMessage = AccountFlow.Text(this, "Your password has been changed.");
        return RedirectToPage();
    }
}
