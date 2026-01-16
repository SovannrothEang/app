using CoreAPI.DTOs;
using CoreAPI.DTOs.Accounts;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;

namespace CoreAPI.Services.Interfaces;

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
    //     CancellationToken cancellationToken = default);
}