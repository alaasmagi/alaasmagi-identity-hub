using Base.Contracts.DTO;
using Domain;
using DTO.Web.DTO;

namespace DTO.Web.Mapper;

public class AppUserClientDtoMapper : IMapper<AppUserClientDto, AppUserClient>
{
    public AppUserClientDto? Map(AppUserClient? entity)
    {
        if (entity is null) return null;

        return new AppUserClientDto
        {
            UserId = entity.UserId,
            ClientId = entity.ClientId,
            Status = entity.Status,
            GrantedAt = entity.GrantedAt,
            GrantedBy = entity.GrantedBy,
            RevokedAt = entity.RevokedAt,
            RevokedBy = entity.RevokedBy,
            RevokeReason = entity.RevokeReason,
            ConsentGivenAt = entity.ConsentGivenAt,
            ConsentIp = entity.ConsentIp
        };
    }

    public IEnumerable<AppUserClientDto> Map(IEnumerable<AppUserClient>? entities)
    {
        return entities?.Select(Map)!;
    }

    public AppUserClient? Map(AppUserClientDto? entity)
    {
        if (entity is null) return null;

        return new AppUserClient
        {
            UserId = entity.UserId,
            ClientId = entity.ClientId,
            Status = entity.Status,
            GrantedAt = entity.GrantedAt,
            GrantedBy = entity.GrantedBy,
            RevokedAt = entity.RevokedAt,
            RevokedBy = entity.RevokedBy,
            RevokeReason = entity.RevokeReason,
            ConsentGivenAt = entity.ConsentGivenAt,
            ConsentIp = entity.ConsentIp
        };
    }

    public IEnumerable<AppUserClient> Map(IEnumerable<AppUserClientDto>? entities)
    {
        return entities?.Select(Map)!;
    }
}