using System.Security.Claims;
using Application.Auth.Responses;
using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.WebUtilities;

namespace Web.Areas.Identity.Pages.Account;

public static class AccountFlow
{
    public static RouteValueDictionary RouteValues(
        Guid clientId,
        string? redirectUri,
        string? tempToken = null,
        string? consentToken = null,
        string? returnUrl = null)
    {
        var values = new RouteValueDictionary
        {
            ["clientId"] = clientId,
            ["redirectUri"] = redirectUri
        };

        if (!string.IsNullOrWhiteSpace(tempToken)) values["tempToken"] = tempToken;
        if (!string.IsNullOrWhiteSpace(consentToken)) values["consentToken"] = consentToken;
        if (!string.IsNullOrWhiteSpace(returnUrl)) values["returnUrl"] = returnUrl;

        return values;
    }

    public static IActionResult HandleLoginResult(
        PageModel page,
        Result<LoginResponse> result,
        Guid clientId,
        string? redirectUri)
    {
        if (!result.IsSuccess || result.Value is null)
        {
            page.ModelState.AddModelError(string.Empty, ToDisplayError(page, result.Error));
            return page.Page();
        }

        var response = result.Value;
        if (response.RequiresTwoFactor)
        {
            return page.RedirectToPage("./LoginWith2fa", RouteValues(clientId, redirectUri, response.TempToken));
        }

        if (response.RequiresConsent)
        {
            return page.RedirectToPage("./Consent", RouteValues(clientId, redirectUri, consentToken: response.ConsentToken));
        }

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            page.ModelState.AddModelError(string.Empty, ToDisplayError(page, response.Error));
            return page.Page();
        }

        return RedirectWithCode(page, redirectUri, response.AuthCode);
    }

    public static IActionResult RedirectWithCode(PageModel page, string? redirectUri, string? authCode)
    {
        if (string.IsNullOrWhiteSpace(redirectUri) || string.IsNullOrWhiteSpace(authCode))
        {
            page.ModelState.AddModelError(string.Empty, Text(page, "The authentication flow could not be completed."));
            return page.Page();
        }

        return new RedirectResult(QueryHelpers.AddQueryString(redirectUri, "code", authCode));
    }

    public static IActionResult RedirectDenied(PageModel page, string? redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            page.ModelState.AddModelError(string.Empty, Text(page, "The authentication flow could not be completed."));
            return page.Page();
        }

        return new RedirectResult(QueryHelpers.AddQueryString(redirectUri, "error", "access_denied"));
    }

    public static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }

    public static string ToDisplayError(string? error)
    {
        return error switch
        {
            "AwaitingApproval" => "Your access is waiting for administrator approval.",
            "AccessRevoked" => "Your access to this application has been revoked.",
            "EmailNotConfirmed" => "Confirm your email address before signing in.",
            "InvalidClient" => "Open sign-in from a registered client application.",
            "InvalidCredentials" => "Invalid login attempt.",
            "InvalidRedirectUri" => "The return address is not valid for this application.",
            "NotInvited" => "You are not invited to access this application.",
            "InvalidConsentToken" => "The consent request is no longer valid.",
            "InvalidTempToken" => "The two-factor login request is no longer valid.",
            "InvalidCode" => "The verification code is invalid.",
            "InvalidRecoveryCode" => "The recovery code is invalid.",
            _ => "The request could not be completed."
        };
    }

    public static string ToDisplayError(PageModel page, string? error)
    {
        return Text(page, ToDisplayError(error));
    }

    public static string Text(PageModel page, string key)
    {
        var localizer = page.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Web.Resources>>();
        return localizer[key];
    }
}
