using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Web.Areas.Admin.Pages.Clients;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList RegistrationTypes { get; private set; } = default!;
    public string? GeneratedSecret { get; private set; }

    public sealed class InputModel
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        [Required]
        public string Name { get; set; } = default!;
        public string? AllowedOrigins { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public ERegistrationType RegistrationType { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        LoadLists();
        var client = id is null ? null : await _context.Clients.FindAsync(id.Value);
        if (client is null) return NotFound();

        Input = new InputModel
        {
            Id = client.Id,
            ClientId = client.ClientId,
            Name = client.Name,
            AllowedOrigins = client.AllowedOrigins,
            IsActive = client.IsActive,
            RegistrationType = client.RegistrationType
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        LoadLists();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = await _context.Clients.FindAsync(Input.Id);
        if (client is null) return NotFound();

        client.Name = Input.Name;
        client.AllowedOrigins = Input.AllowedOrigins;
        client.IsActive = Input.IsActive;
        client.RegistrationType = Input.RegistrationType;
        client.UpdatedBy = User.Identity?.Name ?? "admin";
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostRegenerateSecretAsync()
    {
        LoadLists();
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == Input.Id);
        if (client is null) return NotFound();

        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        client.ClientSecretHash = new PasswordHasher<ClientEntity>().HashPassword(client, secret);
        client.UpdatedBy = User.Identity?.Name ?? "admin";
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        Input = new InputModel
        {
            Id = client.Id,
            ClientId = client.ClientId,
            Name = client.Name,
            AllowedOrigins = client.AllowedOrigins,
            IsActive = client.IsActive,
            RegistrationType = client.RegistrationType
        };
        GeneratedSecret = secret;
        return Page();
    }

    private void LoadLists()
    {
        RegistrationTypes = new SelectList(Enum.GetValues<ERegistrationType>());
    }
}
