using System.ComponentModel.DataAnnotations;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<AppUserEntity> _userManager;

    public IndexModel(UserManager<AppUserEntity> userManager)
    {
        _userManager = userManager;
    }

    public string? Username { get; private set; }
    public bool IsEmailConfirmed { get; private set; }

    public InputModel Input { get; private set; } = new();

    public sealed class InputModel
    {
        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return;
        }

        Username = await _userManager.GetUserNameAsync(user);
        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        Input = new InputModel
        {
            FullName = user.FullName,
            Email = await _userManager.GetEmailAsync(user)
        };
    }
}
