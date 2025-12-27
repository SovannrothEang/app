using CoreAPI.DTOs.Accounts;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllWithCustomerAsync(
        string customerId,
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<IEnumerable<AccountDto>> GetAllWithTenantAsync(
        string tenantId,
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<AccountDto?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);
}