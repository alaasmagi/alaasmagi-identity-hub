using Contracts.DataAccess;
using DataAccess.Context;
using Application.Common.Auth;
using Domain;
using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity;

namespace Web.Services;

public sealed class MainClientResolver
{
    private readonly AppDbContext _dbContext;
    private readonly IClientRepository _clientRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MainClientResolver(
        AppDbContext dbContext,
        IClientRepository clientRepository,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _clientRepository = clientRepository;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Client?> GetMainClientAsync()
    {
        var mainClientName = _configuration["Authentication:MainClientName"]
            ?? Environment.GetEnvironmentVariable("MAIN_CLIENT_NAME")
            ?? "main";

        var clients = await _clientRepository.GetAllClientsAsync();
        return clients.FirstOrDefault(client =>
            client.IsActive &&
            string.Equals(client.Name, mainClientName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Client> EnsureMainClientAsync(string actor)
    {
        var existing = await GetMainClientAsync();
        if (existing is not null)
        {
            return existing;
        }

        var mainClientName = _configuration["Authentication:MainClientName"]
            ?? Environment.GetEnvironmentVariable("MAIN_CLIENT_NAME")
            ?? "main";
        var clientId = Guid.NewGuid();
        var allowedOrigin = GetCurrentOrigin()
            ?? _configuration["Authentication:MainAllowedOrigin"]
            ?? Environment.GetEnvironmentVariable("MAIN_ALLOWED_ORIGIN")
            ?? "http://localhost:5295";

        var entity = new ClientEntity
        {
            Id = clientId,
            Name = mainClientName,
            ClientId = clientId,
            ClientSecretHash = new PasswordHasher<ClientEntity>().HashPassword(null!, Guid.NewGuid().ToString("N")),
            AllowedOrigins = AllowedOriginsHelper.Serialize([allowedOrigin]),
            IsActive = true,
            RegistrationType = ERegistrationType.Open,
            CreatedBy = actor,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = actor,
            UpdatedAt = DateTime.UtcNow,
            ConcurrencyToken = Guid.NewGuid().ToString("N")
        };

        _dbContext.Clients.Add(entity);
        await _dbContext.SaveChangesAsync();

        return new Client
        {
            Id = entity.Id,
            Name = entity.Name,
            ClientId = entity.ClientId,
            ClientSecretHash = entity.ClientSecretHash,
            AllowedOrigins = entity.AllowedOrigins,
            IsActive = entity.IsActive,
            RegistrationType = entity.RegistrationType
        };
    }

    public string GetAdminRedirectUri(HttpRequest request)
    {
        var path = _configuration["Authentication:MainRedirectPath"]
            ?? Environment.GetEnvironmentVariable("MAIN_REDIRECT_PATH")
            ?? "/Admin/Users";

        return $"{request.Scheme}://{request.Host}{path}";
    }

    private string? GetCurrentOrigin()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return request is null ? null : $"{request.Scheme}://{request.Host}";
    }
}
