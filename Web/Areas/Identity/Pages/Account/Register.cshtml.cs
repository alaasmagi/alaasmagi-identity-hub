using System.ComponentModel.DataAnnotations;
using Application.Auth;
using Application.Auth.Requests;
using Application.ExternalAuth;
using Contracts.DataAccess;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Services;

namespace Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IExternalAuthService _externalAuthService;
    private readonly IClientRepository _clientRepository;
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly AdminProvisioningService _adminProvisioningService;

    public RegisterModel(
        IAuthService authService,
        IExternalAuthService externalAuthService,
        IClientRepository clientRepository,
        UserManager<AppUserEntity> userManager,
        AdminProvisioningService adminProvisioningService)
    {
        _authService = authService;
        _externalAuthService = externalAuthService;
        _clientRepository = clientRepository;
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

    public string? ClientName { get; private set; }
    public bool MissingClientContext { get; private set; }
    public IReadOnlyList<string> ExternalProviders { get; private set; } = [];

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = default!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;
    }

    public async Task OnGetAsync()
    {
        await LoadContextAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadContextAsync();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.RegisterAsync(new RegisterRequest(Input.Email, Input.Password, Input.FullName));
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, AccountFlow.ToDisplayError(this, result.Error));
            return Page();
        }

        var isConfiguredAdmin = false;
        var user = await _userManager.FindByIdAsync(result.Value!.UserId.ToString());
        if (user is not null)
        {
            isConfiguredAdmin = await _adminProvisioningService.EnsureConfiguredAdminAsync(user);
        }

        if (isConfiguredAdmin)
        {
            return RedirectToPage("./Login", AccountFlow.RouteValues(ClientId, RedirectUri, returnUrl: ReturnUrl));
        }

        return RedirectToPage("./RegisterConfirmation", AccountFlow.RouteValues(ClientId, RedirectUri, returnUrl: ReturnUrl));
    }

    private async Task LoadContextAsync()
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
