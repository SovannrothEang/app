using System.Linq.Expressions;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface ITenantRepository 
{
    Task<IEnumerable<Tenant>> GetAllAsync(
        // bool childIncluded = false,
        Expression<Func<Tenant, bool>>? filter = null,
        CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<bool> IsExistByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsExistByIdAsync(string id, CancellationToken cancellationToken = default);
}