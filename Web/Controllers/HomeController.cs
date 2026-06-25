using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Login(string? returnUrl = null)
    {
        return RedirectToPage("/Account/Login", new
        {
            area = "Identity",
            returnUrl
        });
    }

    public IActionResult Register(string? returnUrl = null)
    {
        return RedirectToPage("/Account/Register", new
        {
            area = "Identity",
            returnUrl
        });
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

}
