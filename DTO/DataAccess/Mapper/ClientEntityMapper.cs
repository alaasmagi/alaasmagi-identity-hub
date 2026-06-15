using Base.Contracts.DTO;
using Domain;
using DTO.DataAccess.DTO;

namespace DTO.DataAccess.Mapper;

public class ClientEntityMapper : IMapper<Client, ClientEntity>
{
    public Client? Map(ClientEntity? entity)
    {
        if (entity is null) return null;

        return new Client
        {
            Name = entity.Name,
            ClientId = entity.ClientId,
            ClientSecretHash = entity.ClientSecretHash,
            AllowedOrigins = entity.AllowedOrigins,
            IsActive = entity.IsActive,
            RegistrationType = entity.RegistrationType
        };
    }

    public IEnumerable<Client> Map(IEnumerable<ClientEntity>? entities)
    {
        return entities?.Select(Map)!;
    }

    public ClientEntity? Map(Client? entity)
    {
        if (entity is null) return null;

        return new ClientEntity
        {
            Name = entity.Name,
            ClientId = entity.ClientId,
            ClientSecretHash = entity.ClientSecretHash,
            AllowedOrigins = entity.AllowedOrigins,
            IsActive = entity.IsActive,
            RegistrationType = entity.RegistrationType
        };
    }

    public IEnumerable<ClientEntity> Map(IEnumerable<Client>? entities)
    {
        return entities?.Select(Map)!;
    }
}