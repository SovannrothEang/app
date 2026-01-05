using System.Linq.Expressions;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync(
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<Account?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Account>> GetAllWithCustomerAsync(
        string customerId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Account>> GetAllWithTenantAsync(
        string tenantId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);
}