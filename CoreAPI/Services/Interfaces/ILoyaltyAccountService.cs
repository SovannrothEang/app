using CoreAPI.DTOs.LoyaltyAccounts;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ILoyaltyAccountService
{
    Task<IEnumerable<LoyaltyAccountDto>> GetAllWithCustomerAsync(string customerId, CancellationToken ct);
    Task<IEnumerable<LoyaltyAccountDto>> GetAllWithTenantAsync(string tenantId, CancellationToken ct);
    Task<LoyaltyAccountDto?> GetByIdAsync(string tenantId, string customerId, CancellationToken cancellationToken = default);
    Task<LoyaltyAccountDto?> GetByIdWithIncludesAsync(string tenantId, string customerId, CancellationToken cancellationToken = default);
}