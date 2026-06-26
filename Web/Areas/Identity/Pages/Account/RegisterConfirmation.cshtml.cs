using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

public class RegisterConfirmationModel : PageModel
{
    private readonly IConfiguration _configuration;

    public RegisterConfirmationModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty(SupportsGet = true)]
    public Guid ClientId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RedirectUri { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool IsEmailDeliveryConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Brevo:ApiKey"] ?? Environment.GetEnvironmentVariable("BREVO_API_KEY"))
        && !string.IsNullOrWhiteSpace(_configuration["Brevo:SenderEmail"] ?? Environment.GetEnvironmentVariable("BREVO_SENDER_EMAIL"));
}
