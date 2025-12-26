using System.Linq.Expressions;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface IPointTransactionRepository
{
    Task<IEnumerable<PointTransaction>> GetAllAsync(
        Expression<Func<PointTransaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<PointTransaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        Expression<Func<PointTransaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default);
    Task<PointTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}