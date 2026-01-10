using System.Linq.Expressions;
using CoreAPI.DTOs;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<(IEnumerable<Transaction> result, int totalCount)> GetAllAsync(
        PaginationOption option,
        bool childIncluded = false,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetAllByCustomerGlobalAsync(string customerId, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdAsync(string id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetByCustomerIdAsync(string customerId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetByTenantIdAsync(string tenantId,
        CancellationToken cancellationToken = default);
}