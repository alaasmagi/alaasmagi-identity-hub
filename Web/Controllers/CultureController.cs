using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class CultureController : Controller
{
    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "en-US",
        "et-EE",
        "fi-FI"
    };

    [HttpGet]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        if (SupportedCultures.Contains(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });
        }

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/"));
    }
}
