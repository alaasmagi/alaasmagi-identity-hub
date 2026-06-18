using Application.Consent;
using Application.Consent.Requests;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

[Authorize]
public class ConsentModel : PageModel
{
    private readonly IConsentService _consentService;

    public ConsentModel(IConsentService consentService)
    {
        _consentService = consentService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid ClientId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RedirectUri { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ConsentToken { get; set; }

    public string? ClientName { get; private set; }
    public ERegistrationType? RegistrationType { get; private set; }
    public string? Message { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadConsentInfoAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync()
    {
        if (!AccountFlow.TryGetUserId(User, out var userId))
        {
            ModelState.AddModelError(string.Empty, "The consent request requires a signed-in user.");
            await LoadConsentInfoAsync();
            return Page();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _consentService.GrantConsentAsync(
            new GrantConsentRequest(ConsentToken ?? string.Empty, userId, ipAddress));

        if (!result.IsSuccess || result.Value is null)
        {
            Message = result.Error == "NotInvited"
                ? "You are not invited to access this application."
                : null;

            if (Message is null)
            {
                ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(result.Error));
            }

            await LoadConsentInfoAsync();
            return Page();
        }

        if (result.Value.Status == EUserClientStatus.Pending)
        {
            Message = "Your access request is waiting for administrator approval.";
            await LoadConsentInfoAsync();
            return Page();
        }

        return AccountFlow.RedirectWithCode(this, RedirectUri, result.Value.AuthCode);
    }

    public IActionResult OnPostDeny()
    {
        return AccountFlow.RedirectDenied(this, RedirectUri);
    }

    private async Task LoadConsentInfoAsync()
    {
        if (string.IsNullOrWhiteSpace(ConsentToken))
        {
            ModelState.AddModelError(string.Empty, "The consent request is no longer valid.");
            return;
        }

        var result = await _consentService.GetConsentInfoAsync(ConsentToken);
        if (!result.IsSuccess || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(result.Error));
            return;
        }

        ClientName = result.Value.ClientName;
        RegistrationType = result.Value.RegistrationType;
    }
}
