using Application.Admin;
using Application.Admin.Requests;
using Application.Admin.Responses;
using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IAdminService _adminService;

    public IndexModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResponse<UserSummary>? Users { get; private set; }

    public async Task OnGetAsync()
    {
        var result = await _adminService.GetUsersAsync(new GetUsersRequest(Math.Max(1, PageNumber), 25, Search));
        Users = result.IsSuccess ? result.Value : null;
    }
}
