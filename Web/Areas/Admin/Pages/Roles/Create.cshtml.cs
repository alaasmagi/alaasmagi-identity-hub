using System.ComponentModel.DataAnnotations;
using DataAccess.Context;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Web.Areas.Admin.Pages.Roles;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Clients { get; private set; } = default!;

    public sealed class InputModel
    {
        [Required]
        public string Name { get; set; } = default!;
        [Required]
        public Guid ClientId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadClientsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadClientsAsync();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Roles.Add(new AppRoleEntity
        {
            Name = Input.Name,
            NormalizedName = Input.Name.ToUpperInvariant(),
            ClientId = Input.ClientId,
            CreatedBy = User.Identity?.Name ?? "admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = User.Identity?.Name ?? "admin",
            UpdatedAt = DateTime.UtcNow,
            ConcurrencyToken = Guid.NewGuid().ToString("N")
        });
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task LoadClientsAsync()
    {
        var clients = await _context.Clients.AsNoTracking().OrderBy(client => client.Name).ToListAsync();
        Clients = new SelectList(clients, nameof(ClientEntity.ClientId), nameof(ClientEntity.Name));
    }
}
