using System.Linq.Expressions;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync(
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdAsync(string id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetByCustomerIdAsync(string customerId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetByTenantIdAsync(string tenantId,
        CancellationToken cancellationToken = default);
}