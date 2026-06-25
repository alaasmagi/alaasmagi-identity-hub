using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Application.Common.Auth;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Areas.Admin.Pages.Clients;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList RegistrationTypes { get; private set; } = default!;
    public string? GeneratedSecret { get; private set; }
    public Guid? GeneratedClientId { get; private set; }

    public sealed class InputModel
    {
        [Required]
        public string Name { get; set; } = default!;
        public string? AllowedOrigins { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        public ERegistrationType RegistrationType { get; set; }
    }

    public IActionResult OnGet()
    {
        LoadLists();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        LoadLists();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var secret = GenerateSecret();
        var clientId = Guid.NewGuid();
        var client = new ClientEntity
        {
            Id = clientId,
            Name = Input.Name,
            ClientId = clientId,
            ClientSecretHash = new PasswordHasher<ClientEntity>().HashPassword(null!, secret),
            AllowedOrigins = AllowedOriginsHelper.Serialize(ParseAllowedOrigins(Input.AllowedOrigins)),
            IsActive = Input.IsActive,
            RegistrationType = Input.RegistrationType,
            CreatedBy = User.Identity?.Name ?? "admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = User.Identity?.Name ?? "admin",
            UpdatedAt = DateTime.UtcNow,
            ConcurrencyToken = Guid.NewGuid().ToString("N")
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        GeneratedSecret = secret;
        GeneratedClientId = client.ClientId;
        return Page();
    }

    private void LoadLists()
    {
        RegistrationTypes = new SelectList(Enum.GetValues<ERegistrationType>());
    }

    private static string GenerateSecret()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private static List<string> ParseAllowedOrigins(string? origins)
    {
        return (origins ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToList();
    }
}
