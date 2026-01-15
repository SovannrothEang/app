using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Accounts;
using CoreAPI.DTOs.Customers;
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

    public async Task<IEnumerable<AccountDto>> GetAllByCustomerIdForGlobalAsync(
        string customerId,
        string? tenantId, // Focusing on tenantId
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get all accounts by customer: {customerId}", customerId);
        // No transaction was included in this logic
        var (accounts, totalCount) = await _accountRepository.GetAllByCustomerIdForGlobalAsync(
            customerId: customerId,
            option,
            tenantId is null 
                ? null
                : x => x.TenantId == tenantId,
            childIncluded: childIncluded,
            cancellationToken: ct);
        
        var transaction = await _transactionService.GetAllByCustomerIdForGlobalAsync(
            customerId, option, childIncluded, ct);

        // This maybe bad, what if there are many accounts??
        // sortDirection ??= "asc";
        // if (!string.IsNullOrEmpty(sortBy))
        // {
        //     tenants = (sortBy.ToLower(), sortDirection.ToLower()) switch
        //     {
        //         ("balance", "asc") => tenants.OrderBy(e => e.TotalBalance),
        //         ("balance", "desc") => tenants.OrderByDescending(e => e.TotalBalance),
        //         ("name", "asc") => tenants.OrderBy(e => e.TenantName),
        //         ("name", "desc") => tenants.OrderByDescending(e => e.TenantName),
        //         _ => tenants.OrderBy(x => x.Accounts.Select(e => e.CreatedAt)),
        //     };
        // }

        var tenants = accounts
            .GroupBy(a => a.TenantId)
            // .Select(group => new
            // {
            //     TenantId = group.Key,
            //     TenantName = group.First().Tenant!.Name,
            //     TotalBalance = group.Sum(acc => acc.Balance),
            //     Accounts = group.Select(acc => new
            //     {
            //         AccountType = acc.AccountType,
            //         CurrentBalance = acc.Balance,
            //         LastActivity = acc.Transactions,
            //         CreatedAt = acc.CreatedAt,
            //         UpdatedAt = acc.UpdatedAt
            //     })
            // }).AsQueryable();
            .Select(group => new TenantProfileDto
            {
                TenantId = group.Key,
                TenantName = group.First().Tenant.Name,
                TotalBalance = group.Sum(a => a.Balance),
                Accounts = group.First()
            });

        var allTenants = tenants.ToList();
        return new
        {
            TotolPoint = allTenants.Sum(acc => acc.TotalBalance),
            AllTenants = tenants
        });
    }
}