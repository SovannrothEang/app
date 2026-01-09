using CoreAPI.DTOs.Customers;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync(bool childIncluded = false, CancellationToken ct = default);
    Task<IEnumerable<CustomerDto>> GetCustomersPerTenantAsync(
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<CustomerDto> GetByIdAsync(string id, bool childIncluded = false, CancellationToken ct = default);
    Task<CustomerDto> GetByIdInTenantScopeAsync(string id, bool childIncluded = false, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken ct = default);
    Task UpdateAsync(string id, CustomerUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    
    Task<(decimal balance, List<Transaction> list)> GetCustomerBalanceByIdAsync(
        string customerId,
        string tenantId,
        CustomerGetBalanceOptionsDto? options,
        CancellationToken cancellationToken = default);
    
}