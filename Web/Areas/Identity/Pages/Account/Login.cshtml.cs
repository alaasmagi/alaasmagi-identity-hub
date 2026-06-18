using System.ComponentModel.DataAnnotations;
using Application.Auth;
using Application.Auth.Requests;
using Application.ExternalAuth;
using Contracts.DataAccess;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IExternalAuthService _externalAuthService;
    private readonly IClientRepository _clientRepository;

    public LoginModel(
        IAuthService authService,
        IExternalAuthService externalAuthService,
        IClientRepository clientRepository)
    {
        _authService = authService;
        _externalAuthService = externalAuthService;
        _clientRepository = clientRepository;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid ClientId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RedirectUri { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public string? ClientName { get; private set; }
    public IReadOnlyList<string> ExternalProviders { get; private set; } = [];

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        await LoadClientContextAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadClientContextAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.LoginAsync(
            new LoginRequest(Input.Email, Input.Password, ClientId, "cookie", RedirectUri));

        return AccountFlow.HandleLoginResult(this, result, ClientId, RedirectUri);
    }

    private async Task LoadClientContextAsync()
    {
        if (ClientId == Guid.Empty)
        {
            ExternalProviders = [];
            return;
        }

        var client = await _clientRepository.GetByClientIdAsync(ClientId);
        ClientName = client?.Name;

        var providers = await _externalAuthService.GetProvidersAsync(ClientId);
        ExternalProviders = providers.IsSuccess && providers.Value is not null
            ? providers.Value.Providers
            : [];
    }
}
