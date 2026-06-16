using Base.Contracts.DataAccess;
using Domain;

namespace Contracts.DataAccess;

public interface ISecurityEventRepository : IBaseRepository<SecurityEvent>
{
    Task<(List<SecurityEvent> Items, int TotalCount)> GetPagedAsync(Guid? userId, Guid? clientId, int page, int pageSize);
}
