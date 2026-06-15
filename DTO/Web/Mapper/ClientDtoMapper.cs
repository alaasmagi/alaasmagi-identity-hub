using Base.Contracts.DTO;
using Domain;
using DTO.Web.DTO;

namespace DTO.Web.Mapper;

public class ClientDtoMapper : IMapper<ClientDto, Client>
{
	public ClientDto? Map(Client? entity)
	{
		if (entity is null) return null;

		return new ClientDto
		{
			Name = entity.Name,
			ClientId = entity.ClientId,
			AllowedOrigins = entity.AllowedOrigins,
			IsActive = entity.IsActive,
			RegistrationType = entity.RegistrationType
		};
	}

	public IEnumerable<ClientDto> Map(IEnumerable<Client>? entities)
	{
		return entities?.Select(Map)!;
	}

	public Client? Map(ClientDto? entity)
	{
		if (entity is null) return null;

		return new Client
		{
			Name = entity.Name,
			ClientId = entity.ClientId,
			AllowedOrigins = entity.AllowedOrigins,
			IsActive = entity.IsActive,
			RegistrationType = entity.RegistrationType
		};
	}

	public IEnumerable<Client> Map(IEnumerable<ClientDto>? entities)
	{
		return entities?.Select(Map)!;
	}
}