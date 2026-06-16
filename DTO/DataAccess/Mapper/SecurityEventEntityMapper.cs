using Base.Contracts.DTO;
using Domain;
using DTO.DataAccess.DTO;

namespace DTO.DataAccess.Mapper;

public class SecurityEventEntityMapper : IMapper<SecurityEvent, SecurityEventEntity>
{
    public SecurityEvent? Map(SecurityEventEntity? entity)
    {
        if (entity is null) return null;

        return new SecurityEvent
        {
            Id = entity.Id,
            UserId = entity.UserId,
            ClientId = entity.ClientId,
            Type = entity.Type,
            IpAddress = entity.IpAddress,
            UserAgent = entity.UserAgent,
            Timestamp = entity.Timestamp
        };
    }

    public IEnumerable<SecurityEvent> Map(IEnumerable<SecurityEventEntity>? entities)
    {
        return entities?.Select(Map)!;
    }

    public SecurityEventEntity? Map(SecurityEvent? entity)
    {
        if (entity is null) return null;

        return new SecurityEventEntity
        {
            Id = entity.Id,
            UserId = entity.UserId,
            ClientId = entity.ClientId,
            Type = entity.Type,
            IpAddress = entity.IpAddress,
            UserAgent = entity.UserAgent,
            Timestamp = entity.Timestamp
        };
    }

    public IEnumerable<SecurityEventEntity> Map(IEnumerable<SecurityEvent>? entities)
    {
        return entities?.Select(Map)!;
    }
}
