using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Accounts;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using System.Linq.Expressions;

namespace CoreAPI.Services;

public class AccountService(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepository,
    IMapper mapper,
    ILogger<AccountService> logger) : IAccountService
{
    #region Private Fields
    private readonly IRepository<Account> _repository = unitOfWork.GetRepository<Account>();
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AccountService> _logger = logger;
    #endregion

    public async Task<(decimal totalBalance, IEnumerable<TenantProfileDto> profiles)>
        GetAllByCustomerIdForGlobalAsync(
            string customerId,
            PaginationOption option,
            CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get all accounts by customer: {customerId}", customerId);

        // TODO: Maybe make the comparison not case sensitive
        Expression<Func<Account, bool>>? filtering = null;
        if (option.FilterBy is not null && option.FilterValue is not null)
            filtering = option.FilterBy!.ToLower() switch
            {
                "tenantid" => a => a.TenantId == option.FilterValue,
                "name" => a => a.Tenant!.Name == option.FilterValue,
                "typeid" => a => a.AccountTypeId == option.FilterValue,
                "type" => a => a.AccountType!.Name == option.FilterValue,
                _ => null
            };

        var (accounts, totalCount) = await _accountRepository.GetAllByCustomerIdForGlobalAsync(
            customerId,
            option,
            filtering,
            childIncluded: true,
            cancellationToken: ct);
        var totalCountPerTenant = accounts
            .GroupBy(a => a.TenantId)
            .ToDictionary(g => g.Key, g => g.Count());
        var tenantProfile = accounts
            .GroupBy(a => a.TenantId)
            .Select(group =>
            {
                var accountProfiles = group.Select(account => _mapper.Map<AccountProfileDto>(account)).ToList();

                return new TenantProfileDto(
                    group.Key,
                    group.First().Tenant!.Name,
                    group.Sum(acc => acc.Balance),
                    new PagedResult<AccountProfileDto>
                    {
                        Items = accountProfiles,
                        PageNumber = option.Page!.Value,
                        PageSize = option.PageSize!.Value,
                        TotalCount = totalCountPerTenant.GetValueOrDefault(group.Key)
                    }
            );
            }).ToList();

        return (tenantProfile.Sum(p => p.TotalBalance), tenantProfile);
    }
    
    public async Task<decimal> GetTotalBalanceByCustomerIdAsync(
        string customerId,
        CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get total balance for customer: {customerId}", customerId);

        var totalBalance = await _repository.ListAsync(
            filter: a => a.CustomerId == customerId,
            select: a => a.Balance,
            cancellationToken: ct)
            ?? throw new BadHttpRequestException("Unable to calculate total balance.");
        return totalBalance.Sum();
    }
}