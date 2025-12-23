using System.Linq.Expressions;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface ITenantRepository : IRepository<Tenant, string>
{
    Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetAllWithFiltering(
        Expression<Func<Tenant, bool>> filtering,
        CancellationToken cancellationToken = default);
}