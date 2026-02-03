using Application.DTOs.Customers;
using Application.DTOs.Transactions;
using Domain.Shared;

namespace Application.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetPaginatedResultsAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<PagedResult<CustomerDto>> GetPaginatedResultsForTenantAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default);
    
    /// <summary>
    /// Get all customer with ID, which retrieve by Global user, can be Customer themselves or Admin 
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="option">Pagination option which's for the child that being included</param>
    /// <param name="childIncluded">Include Accounts, Performer, User</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<CustomerDto> GetByIdForGlobalAsync(
        string customerId,
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<CustomerDto> GetByIdForTenantAsync(string id, bool childIncluded = false, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken ct = default);
    Task UpdateAsync(string id, CustomerUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    
    /// <summary>
    /// Get the customer balance by customerId and tenantId
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="tenantId"></param>
    /// <param name="pageOption">Pagination option for the transactions</param>
    /// <param name="childIncluded"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<(decimal balance, PagedResult<TransactionDto> result)> GetCustomerBalanceByIdAsync(
        string customerId,
        string tenantId,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken ct = default);
    
}