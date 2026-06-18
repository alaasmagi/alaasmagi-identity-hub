using System.ComponentModel.DataAnnotations;
using DataAccess.Context;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Web.Areas.Admin.Pages.Roles;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Clients { get; private set; } = default!;

    public sealed class InputModel
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = default!;
        [Required]
        public Guid ClientId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        await LoadClientsAsync();
        var role = id is null ? null : await _context.Roles.FindAsync(id.Value);
        if (role is null) return NotFound();

        Input = new InputModel
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            ClientId = role.ClientId
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadClientsAsync();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var role = await _context.Roles.FindAsync(Input.Id);
        if (role is null) return NotFound();

        role.Name = Input.Name;
        role.NormalizedName = Input.Name.ToUpperInvariant();
        role.ClientId = Input.ClientId;
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }

    private async Task LoadClientsAsync()
    {
        var clients = await _context.Clients.AsNoTracking().OrderBy(client => client.Name).ToListAsync();
        Clients = new SelectList(clients, nameof(ClientEntity.ClientId), nameof(ClientEntity.Name));
    }
}
