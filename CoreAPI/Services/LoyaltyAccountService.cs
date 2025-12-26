using AutoMapper;
using CoreAPI.DTOs.LoyaltyAccounts;
using CoreAPI.Repositories;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class LoyaltyAccountService(ILoyaltyAccountRepository accountRepository, IMapper mapper) : ILoyaltyAccountService
{
    private readonly ILoyaltyAccountRepository _accountRepository = accountRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<LoyaltyAccountDto>> GetAllWithCustomerAsync(
        string customerId,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var account = await _accountRepository.GetAllWithCustomerAsync(
            customerId: customerId,
            childIncluded: childIncluded,
            cancellationToken: ct);
        
        return account.Select(a => _mapper.Map<LoyaltyAccountDto>(a));
    }

    public async Task<IEnumerable<LoyaltyAccountDto>> GetAllWithTenantAsync(
        string tenantId,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var accounts = await _accountRepository.GetAllWithTenantAsync(
            tenantId: tenantId,
            childIncluded: childIncluded,
            cancellationToken: ct);
        return accounts.Select(a => _mapper.Map<LoyaltyAccountDto>(a));
    }

    public async Task<LoyaltyAccountDto?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId, 
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByTenantAndCustomerAsync(
            tenantId, customerId, childIncluded, cancellationToken);
        return _mapper.Map<LoyaltyAccountDto>(account);
    }
}