using System.Linq.Expressions;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface IAccountRepository
{
    /// <summary>
    /// Get all accounts available in the tenant scope
    /// </summary>
    /// <param name="option">
    /// Pagination options, e.g. Page, PageSize, SortBy, SortDirection, etc
    /// </param>
    /// <param name="filtering">
    /// Filtering by using Expression of LINQ (object: Account)
    /// </param>
    /// <param name="childIncluded">
    /// Include Tenant, Customer, and Performer. No Transaction was included, and AccountType is being included by default
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(IEnumerable<Account> result, int totalCount)> GetAllAsync(
        PaginationOption option,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all accounts for customer by id, retrieve by Tenant
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="filtering">
    /// Filtering by using Expression of LINQ (object: Account)
    /// </param>
    /// <param name="childIncluded">
    /// Include Tenant, Customer, and Performer. No Transaction was included, and AccountType is being included by default
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Account>> GetAllByCustomerIdAsync(
        string customerId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all of customer's accounts, by ID (Ignore Global query, retrieve by Customer, or Admin)
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="option">
    /// Pagination options, e.g. Page, PageSize, SortBy, SortDirection, etc
    /// </param>
    /// <param name="filtering">
    /// Filtering by using Expression of LINQ (object: Account)
    /// </param>
    /// <param name="childIncluded">
    /// Include Tenant, Customer, and Performer. No Transaction was included, and AccountType is being included by default
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A ready paginated IEnumerable of accounts, and a total number of accounts
    /// </returns>
    Task<(IEnumerable<Account> result, int totalCount)> GetAllByCustomerIdForGlobalAsync(
        string customerId,
        PaginationOption option,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);
}