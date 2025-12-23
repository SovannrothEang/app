using CoreAPI.DTOs.LoyaltyAccounts;
using CoreAPI.Models;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class LoyaltyAccountService : ILoyaltyAccountService
{
    public Task<IEnumerable<LoyaltyAccountDto>> GetAllWithCustomerAsync(string customerId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<LoyaltyAccountDto>> GetAllWithTenantAsync(string tenantId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<LoyaltyAccountDto?> GetByIdAsync(string tenantId, string customerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LoyaltyAccountDto?> GetByIdWithIncludesAsync(string tenantId, string customerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}