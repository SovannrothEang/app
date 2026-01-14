using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync(bool childIncluded = false, CancellationToken ct = default);
    Task<IEnumerable<CustomerDto>> GetCustomersForTenantAsync(
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<CustomerDto> GetByIdForCustomerAsync(string id, bool childIncluded = false, CancellationToken ct = default);
    Task<CustomerDto> GetByIdForTenantAsync(string id, bool childIncluded = false, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken ct = default);
    Task UpdateAsync(string id, CustomerUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    
    Task<(decimal balance, PagedResult<TransactionDto> list)> GetCustomerBalanceByIdAsync(
        string customerId,
        string tenantId,
        PaginationOption pageOption,
        CancellationToken cancellationToken = default);
    
}