using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Transactions;

namespace CoreAPI.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllForGobalAsync(
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<IEnumerable<CustomerDto>> GetAllForTenantAsync(
        bool childIncluded = false,
        CancellationToken ct = default);
    
    /// <summary>
    /// Get all customer with ID, which retrieve by Global user, can be Customer themselves or Admin 
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="option">
    /// Pagination option which's for the child that being included
    /// </param>
    /// <param name="childIncluded">
    /// Include Accounts, Performer, User
    /// </param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<CustomerDto> GetByIdForCustomerAsync(
        string customerId,
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<CustomerDto> GetByIdForTenantAsync(string id, bool childIncluded = false, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken ct = default);
    Task UpdateAsync(string id, CustomerUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    
    Task<(decimal balance, PagedResult<TransactionDto> result)> GetCustomerBalanceByIdAsync(
        string customerId,
        string tenantId,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);
    
}