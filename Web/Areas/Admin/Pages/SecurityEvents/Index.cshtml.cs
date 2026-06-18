using Application.Admin;
using Application.Admin.Requests;
using Application.Common;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.SecurityEvents;

public class IndexModel : PageModel
{
    private readonly IAdminService _adminService;

    public IndexModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ClientId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResponse<SecurityEvent>? SecurityEvents { get; private set; }

    public async Task OnGetAsync()
    {
        var result = await _adminService.GetSecurityEventsAsync(
            new GetSecurityEventsRequest(UserId, ClientId, Math.Max(1, PageNumber), 50));
        SecurityEvents = result.IsSuccess ? result.Value : null;
    }
}
