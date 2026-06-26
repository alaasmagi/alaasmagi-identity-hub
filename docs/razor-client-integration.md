# Razor Client Integration

This flow is for another ASP.NET Core Razor Pages app, such as `invoice-service`, using identity-hub as the central login UI.

The browser flow is:

```text
client Razor page
-> identity-hub /Identity/Account/Login
-> client Razor callback page with ?code=...
-> client server exchanges code for claims
-> client creates its own local auth cookie
```

## Identity-Hub Client Setup

Create or update a row in the identity-hub `Clients` table.

For `invoice-service`:

```text
Name: invoice-service
ClientId: 7e8c02c8-0468-43f0-bc61-3558567dbdf9
AllowedOrigins: https://invoice.alaasmagi.dev
IsActive: true
```

`AllowedOrigins` is the origin only. It is not the full callback URL.

Allowed:

```text
https://invoice.alaasmagi.dev
```

The callback may still be:

```text
https://invoice.alaasmagi.dev/Auth/Callback
```

The identity-hub code validates the callback by comparing the callback origin against `Clients.AllowedOrigins`.

## Client App Configuration

In the Razor client app:

```env
IdentityHub__BaseUrl=https://identity.alaasmagi.dev
IdentityHub__ClientId=7e8c02c8-0468-43f0-bc61-3558567dbdf9
IdentityHub__ClientSecret=<plain-client-secret>
```

The plain client secret must match the hash stored in identity-hub. If the plain value is lost, rotate/regenerate it.

## Login Page

From the client app, redirect users to identity-hub's Razor login page.

Example page handler:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

public sealed class LoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public LoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult OnPost()
    {
        var identityBaseUrl = _configuration["IdentityHub:BaseUrl"]!;
        var clientId = _configuration["IdentityHub:ClientId"]!;
        var callbackUrl = Url.PageLink("/Auth/Callback")!;

        var loginUrl = QueryHelpers.AddQueryString(
            $"{identityBaseUrl.TrimEnd('/')}/Identity/Account/Login",
            new Dictionary<string, string?>
            {
                ["clientId"] = clientId,
                ["redirectUri"] = callbackUrl
            });

        return Redirect(loginUrl);
    }
}
```

This produces a URL like:

```text
https://identity.alaasmagi.dev/Identity/Account/Login?clientId=7e8c02c8-0468-43f0-bc61-3558567dbdf9&redirectUri=https%3A%2F%2Finvoice.alaasmagi.dev%2FAuth%2FCallback
```

## Callback Page

Create a callback page in the client app:

```text
Pages/Auth/Callback.cshtml
Pages/Auth/Callback.cshtml.cs
```

`Callback.cshtml.cs`:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public sealed class CallbackModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CallbackModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(string? code, string? error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            ModelState.AddModelError(string.Empty, error);
            return Page();
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(string.Empty, "Missing authentication code.");
            return Page();
        }

        var identityBaseUrl = _configuration["IdentityHub:BaseUrl"]!;
        var client = _httpClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"{identityBaseUrl.TrimEnd('/')}/api/auth/external/token/exchange",
            new
            {
                code,
                clientId = _configuration["IdentityHub:ClientId"],
                clientSecret = _configuration["IdentityHub:ClientSecret"]
            });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Could not complete sign-in.");
            return Page();
        }

        var result = await response.Content.ReadFromJsonAsync<ClaimsResponse>();
        var claims = result?.Claims
            .Select(claim => new Claim(claim.Type, claim.Value))
            .ToList() ?? [];

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToPage("/Index");
    }

    private sealed class ClaimsResponse
    {
        public List<ClaimDto> Claims { get; set; } = [];
    }

    private sealed class ClaimDto
    {
        public string Type { get; set; } = default!;
        public string Value { get; set; } = default!;
    }
}
```

`Callback.cshtml` can be minimal:

```html
@page
@model CallbackModel

<h1>Signing in</h1>

<div asp-validation-summary="All"></div>
```

## Client Cookie Setup

The client app must have cookie authentication configured:

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });

builder.Services.AddHttpClient();
```

And in the request pipeline:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

## Important Notes

Identity-hub's provider callback URLs for Google and Microsoft are still configured on identity-hub itself:

```text
https://identity.alaasmagi.dev/signin-google
https://identity.alaasmagi.dev/signin-microsoft
```

The client app callback is separate:

```text
https://invoice.alaasmagi.dev/Auth/Callback
```

If identity-hub returns:

```text
The return address is not valid for this application.
```

then `Clients.AllowedOrigins` does not match the origin of the `redirectUri`.

