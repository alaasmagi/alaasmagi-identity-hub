using Base.DataAccess.EF;
using DataAccess.Context;
using Domain;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;

namespace DataAccess.Repository;

public class ClientRepository : BaseRepository<Client, ClientEntity, ClientEntityMapper>
{
    public ClientRepository(AppDbContext repositoryDbContext, ClientEntityMapper repositoryMapper) 
        : base(repositoryDbContext, repositoryMapper)
    {
    }
}