using Application.DTOs.Tenants;
using Domain.Shared;

namespace Application.Services;

public interface IAccountService
{
    /// <summary>
    /// Get all accounts by using Customer ID, Ignore Global Query
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="tenantId">
    /// Filtering purpose
    /// </param>
    /// <param name="option">
    /// Pagination options, e.g. Page, PageSize, SortBy, SortDirection, etc
    /// </param>
    /// <param name="childIncluded"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<(decimal totalBalance, IEnumerable<TenantProfileDto> profiles)>
        GetAllByCustomerIdForGlobalAsync(
            string customerId,
            PaginationOption option,
            //bool childIncluded = false,
            CancellationToken ct = default);
    // Task<AccountDto?> GetByTenantAndCustomerAsync(
    //     string tenantId,
    //     string customerId,
    //     bool childIncluded = false,
    //     Cancell
    
    /// <summary>
    /// Get total balance by Customer ID
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<decimal> GetTotalBalanceByCustomerIdAsync(
        string customerId,
        CancellationToken ct = default);
}