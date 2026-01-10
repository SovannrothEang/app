using AutoMapper;
using CoreAPI.DTOs.Accounts;
using CoreAPI.Repositories;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class AccountService(IAccountRepository accountRepository, IMapper mapper, ILogger<AccountService> logger) : IAccountService
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AccountService> _logger = logger;

    public async Task<IEnumerable<AccountDto>> GetAllWithCustomerAsync(
        string customerId,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get all accounts by customer: {customerId}", customerId);
        var account = await _accountRepository.GetAllWithCustomerAsync(
            customerId: customerId,
            childIncluded: childIncluded,
            cancellationToken: ct);
        
        return account.Select(a => _mapper.Map<AccountDto>(a));
    }

    public async Task<IEnumerable<AccountDto>> GetAllWithTenantAsync(
        string tenantId,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get all accounts for tenant with id: {tenantId}", tenantId);
        var accounts = await _accountRepository.GetAllWithTenantAsync(
            tenantId: tenantId,
            childIncluded: childIncluded,
            cancellationToken: ct);
        return accounts.Select(a => _mapper.Map<AccountDto>(a));
    }

    public async Task<AccountDto?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId, 
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByTenantAndCustomerAsync(
            tenantId, customerId, childIncluded,  cancellationToken);
        return _mapper.Map<AccountDto>(account);
    }
}