using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;

namespace Web.Services;

public sealed class BootstrapAdminService
{
    private const string AdminRoleName = "Admin";

    private readonly UserManager<AppUserEntity> _userManager;
    private readonly RoleManager<AppRoleEntity> _roleManager;
    private readonly MainClientResolver _mainClientResolver;
    private readonly IConfiguration _configuration;

    public BootstrapAdminService(
        UserManager<AppUserEntity> userManager,
        RoleManager<AppRoleEntity> roleManager,
        MainClientResolver mainClientResolver,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mainClientResolver = mainClientResolver;
        _configuration = configuration;
    }

    public async Task EnsureBootstrapAdminAsync(AppUserEntity user)
    {
        var bootstrapEmail = _configuration["Authentication:BootstrapAdminEmail"]
            ?? Environment.GetEnvironmentVariable("BOOTSTRAP_ADMIN_EMAIL");

        if (string.IsNullOrWhiteSpace(bootstrapEmail) ||
            !string.Equals(user.Email, bootstrapEmail, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var mainClient = await _mainClientResolver.EnsureMainClientAsync(user.Email ?? "bootstrap");

        var role = await _roleManager.FindByNameAsync(AdminRoleName);
        if (role is null)
        {
            var actor = user.Email ?? "bootstrap";
            role = new AppRoleEntity
            {
                Name = AdminRoleName,
                NormalizedName = AdminRoleName.ToUpperInvariant(),
                ClientId = mainClient.ClientId,
                CreatedBy = actor,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = actor,
                UpdatedAt = DateTime.UtcNow,
                ConcurrencyToken = Guid.NewGuid().ToString("N")
            };

            await _roleManager.CreateAsync(role);
        }

        if (!await _userManager.IsInRoleAsync(user, AdminRoleName))
        {
            await _userManager.AddToRoleAsync(user, AdminRoleName);
        }
    }
}
