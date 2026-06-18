using Application.Common.Abstractions;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;

namespace Web.Services;

public sealed class SecurityEventService : ISecurityEventService
{
    private readonly AppDbContext _dbContext;

    public SecurityEventService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(ESecurityEventType type, AppUser user, Client? client, string? ip, string? userAgent)
    {
        _dbContext.SecurityEvents.Add(new SecurityEventEntity
        {
            UserId = user.Id,
            ClientId = client?.ClientId,
            Type = type,
            IpAddress = ip,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            CreatedBy = user.Id.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = user.Id.ToString(),
            UpdatedAt = DateTime.UtcNow,
            ConcurrencyToken = Guid.NewGuid().ToString("N")
        });

        await _dbContext.SaveChangesAsync();
    }
}
