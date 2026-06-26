using System.Text;
using Application.Auth;
using Application.Auth.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

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
        var confirmationToken = token ?? DecodeCode(code);
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

    private static string? DecodeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
