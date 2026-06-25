using Application.Auth;
using Application.Auth.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly IAuthService _authService;

    public ConfirmEmailModel(IAuthService authService)
    {
        _authService = authService;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid userId, string? token = null, string? code = null)
    {
        var confirmationToken = token ?? code;
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(confirmationToken))
        {
            return RedirectToPage("/Index");
        }

        var result = await _authService.ConfirmEmailAsync(new ConfirmEmailRequest(userId, confirmationToken));
        StatusMessage = result.IsSuccess
            ? AccountFlow.Text(this, "Thank you for confirming your email.")
            : AccountFlow.Text(this, "Error confirming your email.");
        return Page();
    }
}
