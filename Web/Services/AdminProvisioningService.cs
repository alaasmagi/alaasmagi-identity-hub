using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;

namespace Web.Services;

public sealed class AdminProvisioningService
{
    private const string AdminRoleName = "Admin";

    private readonly UserManager<AppUserEntity> _userManager;
    private readonly RoleManager<AppRoleEntity> _roleManager;
    private readonly MainClientResolver _mainClientResolver;
    private readonly IConfiguration _configuration;

    public AdminProvisioningService(
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

    public async Task<bool> EnsureConfiguredAdminAsync(AppUserEntity user)
    {
        if (!IsConfiguredAdmin(user))
        {
            return false;
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        await EnsureAdminRoleAsync(user.Email ?? "admin");

        if (!await _userManager.IsInRoleAsync(user, AdminRoleName))
        {
            await _userManager.AddToRoleAsync(user, AdminRoleName);
        }

        return true;
    }

    public async Task EnsureAdminRoleAsync(string actor)
    {
        var mainClient = await _mainClientResolver.EnsureMainClientAsync(actor);

        var role = await _roleManager.FindByNameAsync(AdminRoleName);
        if (role is not null)
        {
            return;
        }

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

    private bool IsConfiguredAdmin(AppUserEntity user)
    {
        var adminEmail = _configuration["Authentication:AdminEmail"]
            ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL");

        return !string.IsNullOrWhiteSpace(adminEmail) &&
            string.Equals(user.Email, adminEmail, StringComparison.OrdinalIgnoreCase);
    }
}
