using System.ComponentModel.DataAnnotations;
using Application.Common.Abstractions;
using Application.Common.Auth;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account;

public class ResendEmailConfirmationModel : PageModel
{
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly IEmailService _emailService;

    public ResendEmailConfirmationModel(UserManager<AppUserEntity> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is not null && !await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendEmailConfirmationAsync(user.ToDomainUser(), token);
        }

        return RedirectToPage("./RegisterConfirmation");
    }
}
