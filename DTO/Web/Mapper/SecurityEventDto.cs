using Base.Contracts.DTO;
using Domain;
using DTO.Web.DTO;

namespace DTO.Web.Mapper;

public class SecurityEventDtoMapper : IMapper<SecurityEventDto, SecurityEvent>
{
	public SecurityEventDto? Map(SecurityEvent? entity)
	{
		if (entity is null) return null;

		return new SecurityEventDto
		{
			ClientId = entity.ClientId,
			Type = entity.Type,
			IpAddress = entity.IpAddress,
			UserAgent = entity.UserAgent,
			Timestamp = entity.Timestamp
		};
	}

	public IEnumerable<SecurityEventDto> Map(IEnumerable<SecurityEvent>? entities)
	{
		return entities?.Select(Map)!;
	}

	public SecurityEvent? Map(SecurityEventDto? entity)
	{
		if (entity is null) return null;

		return new SecurityEvent
		{
			ClientId = entity.ClientId,
			Type = entity.Type,
			IpAddress = entity.IpAddress,
			UserAgent = entity.UserAgent,
			Timestamp = entity.Timestamp
		};
	}

	public IEnumerable<SecurityEvent> Map(IEnumerable<SecurityEventDto>? entities)
	{
		return entities?.Select(Map)!;
	}
}