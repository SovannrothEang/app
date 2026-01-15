using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Accounts;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class AccountService(
    IAccountRepository accountRepository,
    IMapper mapper,
    ITransactionService transactionService,
    ILogger<AccountService> logger) : IAccountService
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ILogger<AccountService> _logger = logger;

    public async Task<(decimal totalBalance, IEnumerable<TenantProfileDto> profiles)>
        GetAllByCustomerIdForGlobalAsync(
            string customerId,
            string? tenantId, // Focusing on tenantId
            PaginationOption option,
            bool childIncluded = false,
            CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get all accounts by customer: {customerId}", customerId);
        var (accounts, totalCount) = await _accountRepository.GetAllByCustomerIdForGlobalAsync(
            customerId: customerId,
            option,
            tenantId is null
                ? null
                : x => x.TenantId == tenantId,
            childIncluded: childIncluded,
            cancellationToken: ct);
        var tenantProfile = accounts
            .GroupBy(a => a.TenantId)
            .Select(group =>
            {
                var accountProfiles = group.Select(account => new AccountCustomerProfileDto(
                    account.AccountType!.Name,
                    account.Balance,
                    new PagedResult<TransactionDto>
                    {
                        Items = account.Transactions.Select(t => _mapper.Map<TransactionDto>(t)).ToList(),
                        PageNumber = option.Page!.Value,
                        PageSize = option.PageSize!.Value,
                        TotalCount = totalCount
                    },
                    account.CreatedAt,
                    account.UpdatedAt)
                ).ToList();

                return new TenantProfileDto(
                    group.Key,
                    group.First().Tenant!.Name,
                    group.Sum(acc => acc.Balance),
                    accountProfiles
                );
            }).ToList();

        return (tenantProfile.Sum(p => p.TotalBalance), tenantProfile);
    }
}