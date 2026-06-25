using System.ComponentModel.DataAnnotations;
using Application.Auth;
using Application.Auth.Requests;
using Application.ExternalAuth;
using Contracts.DataAccess;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Services;

namespace Web.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IExternalAuthService _externalAuthService;
    private readonly IClientRepository _clientRepository;
    private readonly SignInManager<AppUserEntity> _signInManager;
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly AdminProvisioningService _adminProvisioningService;

    public LoginModel(
        IAuthService authService,
        IExternalAuthService externalAuthService,
        IClientRepository clientRepository,
        SignInManager<AppUserEntity> signInManager,
        UserManager<AppUserEntity> userManager,
        AdminProvisioningService adminProvisioningService)
    {
        _authService = authService;
        _externalAuthService = externalAuthService;
        _clientRepository = clientRepository;
        _signInManager = signInManager;
        _userManager = userManager;
        _adminProvisioningService = adminProvisioningService;
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
    public bool MissingClientContext { get; private set; }
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

        if (ClientId != Guid.Empty)
        {
            var clientFlowUser = await _userManager.FindByEmailAsync(Input.Email);
            if (clientFlowUser is not null)
            {
                await _adminProvisioningService.EnsureConfiguredAdminAsync(clientFlowUser);
            }

            var result = await _authService.LoginAsync(
                new LoginRequest(Input.Email, Input.Password, ClientId, "cookie", RedirectUri));

            return AccountFlow.HandleLoginResult(this, result, ClientId, RedirectUri);
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(this, "InvalidCredentials"));
            return Page();
        }

        await _adminProvisioningService.EnsureConfiguredAdminAsync(user);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: true);
        if (signInResult.Succeeded)
        {
            await _signInManager.SignInAsync(user, Input.RememberMe);
            return LocalRedirect(ReturnUrl ?? "/Admin/Users");
        }

        if (signInResult.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(this, "EmailNotConfirmed"));
            return Page();
        }

        ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(this, "InvalidCredentials"));
        return Page();
    }

    private async Task LoadClientContextAsync()
    {
        if (ClientId == Guid.Empty)
        {
            MissingClientContext = true;
            ExternalProviders = [];
            return;
        }

        var client = await _clientRepository.GetByClientIdAsync(ClientId);
        ClientName = client?.Name;
        MissingClientContext = client is null;

        var providers = await _externalAuthService.GetProvidersAsync(ClientId);
        ExternalProviders = providers.IsSuccess && providers.Value is not null
            ? providers.Value.Providers
            : [];
    }
}
