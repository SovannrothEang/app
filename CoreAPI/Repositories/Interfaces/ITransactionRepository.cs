using System.Linq.Expressions;
using CoreAPI.DTOs;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface ITransactionRepository
{
    /// <summary>
    /// This is for the SuperAdmin to get all transactions
    /// </summary>
    /// <param name="option">
    /// Pagination options, e.g. Page, PageSize, SortBy, SortDirection, etc
    /// Sort by: balance, type (Transaction type), occureedat, and createdat (default)
    /// </param>
    /// <param name="childIncluded">
    /// Include Referencer, and Performer. By default, it included TransactionType
    /// </param>
    /// <param name="filtering"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A set of paginated ready Transaction, and a number of total transactions
    /// </returns>
    Task<(IEnumerable<Transaction> result, int totalCount)> GetAllForGlobalAsync(
        PaginationOption option,
        bool childIncluded = false,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// This is for the Tenant scope to retrieve their customer transaction (had accounts)
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="customerId"></param>
    /// <param name="childIncluded"></param>
    /// <param name="filtering"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Transaction>> GetAllByTenantAsync(
        string tenantId,
        string customerId,
        bool childIncluded,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<Transaction> list, int totalCount)> GetAllByCustomerIdAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="option">
    /// Pagination options, e.g. Page, PageSize, SortBy, SortDirection, etc
    /// Sort by: balance, type (Transaction type), occureedat, and createdat (default)
    /// </param>
    /// <param name="childIncluded">
    /// Include Referencer, and Performer. By default, it included TransactionType
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(IEnumerable<Transaction> list, int totalCount)> GetAllByCustomerIdForGlobalAsync(
        string customerId,
        PaginationOption option,
        bool childIncluded,
        CancellationToken cancellationToken = default);
}