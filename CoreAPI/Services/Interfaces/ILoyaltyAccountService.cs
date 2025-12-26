using CoreAPI.DTOs.LoyaltyAccounts;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ILoyaltyAccountService
{
    Task<IEnumerable<LoyaltyAccountDto>> GetAllWithCustomerAsync(
        string customerId,
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<IEnumerable<LoyaltyAccountDto>> GetAllWithTenantAsync(
        string tenantId,
        bool childIncluded = false,
        CancellationToken ct = default);
    Task<LoyaltyAccountDto?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);
}