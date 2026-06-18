using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly MainClientResolver _mainClientResolver;

    public HomeController(MainClientResolver mainClientResolver)
    {
        _mainClientResolver = mainClientResolver;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        return await RedirectToAccountPageAsync("/Account/Login", returnUrl);
    }

    public async Task<IActionResult> Register(string? returnUrl = null)
    {
        return await RedirectToAccountPageAsync("/Account/Register", returnUrl);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<IActionResult> RedirectToAccountPageAsync(string page, string? returnUrl)
    {
        var mainClient = await _mainClientResolver.EnsureMainClientAsync("system");

        var redirectUri = string.IsNullOrWhiteSpace(returnUrl)
            ? _mainClientResolver.GetAdminRedirectUri(Request)
            : $"{Request.Scheme}://{Request.Host}{returnUrl}";

        return RedirectToPage(page, new
        {
            area = "Identity",
            clientId = mainClient.ClientId,
            redirectUri,
            returnUrl
        });
    }
}
