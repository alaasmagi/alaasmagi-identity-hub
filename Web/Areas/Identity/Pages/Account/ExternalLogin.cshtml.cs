using Application.ExternalAuth;
using Application.ExternalAuth.Requests;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

public class ExternalLoginModel : PageModel
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly SignInManager<AppUserEntity> _signInManager;

    public ExternalLoginModel(
        IExternalAuthService externalAuthService,
        SignInManager<AppUserEntity> signInManager)
    {
        _externalAuthService = externalAuthService;
        _signInManager = signInManager;
    }

    [BindProperty(SupportsGet = true)]
    public Guid ClientId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RedirectUri { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string? TenantId { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet() => RedirectToPage("./Login", AccountFlow.RouteValues(ClientId, RedirectUri, returnUrl: ReturnUrl));

    public IActionResult OnPost(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider) || ClientId == Guid.Empty || string.IsNullOrWhiteSpace(RedirectUri))
        {
            ErrorMessage = AccountFlow.Text(this, "Invalid external login request.");
            return RedirectToPage("./Login", AccountFlow.RouteValues(ClientId, RedirectUri, returnUrl: ReturnUrl));
        }

        var redirectUrl = Url.Page(
            "./ExternalLogin",
            pageHandler: "Callback",
            values: new { provider, clientId = ClientId, redirectUri = RedirectUri, tenantId = TenantId, returnUrl = ReturnUrl })
            ?? throw new InvalidOperationException("External login callback URL could not be generated.");

        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        properties.Items["clientId"] = ClientId.ToString();
        properties.Items["redirectUri"] = RedirectUri;
        if (!string.IsNullOrWhiteSpace(TenantId))
        {
            properties.Items["tenant"] = TenantId;
            properties.Parameters["tenant"] = TenantId;
        }

        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(
        string provider,
        Guid clientId,
        string redirectUri,
        string? tenantId = null,
        string? returnUrl = null,
        string? remoteError = null)
    {
        ClientId = clientId;
        RedirectUri = redirectUri;
        ReturnUrl = returnUrl;

        if (!string.IsNullOrWhiteSpace(remoteError))
        {
            ErrorMessage = AccountFlow.Text(this, "Error from external provider:") + $" {remoteError}";
            return RedirectToPage("./Login", AccountFlow.RouteValues(clientId, redirectUri, returnUrl: returnUrl));
        }

        var result = await _externalAuthService.HandleExternalCallbackAsync(
            new ExternalCallbackRequest(provider, clientId, redirectUri, tenantId));

        if (!result.IsSuccess || result.Value is null)
        {
            ErrorMessage = AccountFlow.ToDisplayError(this, result.Error);
            return RedirectToPage("./Login", AccountFlow.RouteValues(clientId, redirectUri, returnUrl: returnUrl));
        }

        if (result.Value.RequiresConsent)
        {
            return RedirectToPage("./Consent", AccountFlow.RouteValues(
                clientId,
                redirectUri,
                consentToken: result.Value.ConsentToken,
                returnUrl: returnUrl));
        }

        return AccountFlow.RedirectWithCode(this, redirectUri, result.Value.Code);
    }
}
