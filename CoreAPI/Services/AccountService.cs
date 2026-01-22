using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Accounts;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CoreAPI.Services;

public class AccountService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AccountService> logger) : IAccountService
{
    #region Private Fields
    private readonly IRepository<Account> _repository = unitOfWork.GetRepository<Account>();
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AccountService> _logger = logger;
    #endregion

    public async Task<(decimal totalBalance, IEnumerable<TenantProfileDto> profiles)>
        GetAllByCustomerIdForGlobalAsync(
            string customerId,
            PaginationOption option,
            CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 1;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get all accounts by customer: {customerId}", customerId);

        // TODO: Maybe make the comparison not case sensitive
        Expression<Func<Account, bool>> filtering = q => q.CustomerId == customerId;
        if (option.FilterBy is not null && option.FilterValue is not null)
            filtering = option.FilterBy!.ToLower() switch
            {
                "tenantid" => a => a.CustomerId == customerId && a.TenantId == option.FilterValue,
                "name" => a => a.CustomerId == customerId && a.Tenant!.Name == option.FilterValue,
                "typeid" => a => a.CustomerId == customerId && a.AccountTypeId == option.FilterValue,
                "type" => a => a.CustomerId == customerId && a.AccountType!.Name == option.FilterValue,
                _ => filtering
            };

        //var (accounts, totalCount) = await _accountRepository.GetAllByCustomerIdForGlobalAsync(
        //    customerId,
        //    option,
        //    filtering,
        //    childIncluded: true,
        //    cancellationToken: ct);
        var (accounts, totalCount) = await _repository.GetPagedResultAsync(
            option: option,
            ignoreQueryFilters: true,
            filter: filtering,
            includes: q => q
                .Include(e => e.AccountType)
                .Include(e => e.Customer)
                .Include(e => e.Tenant)
                .Include(e => e.Performer)
                .Include(e => e.Transactions
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(1)),
            orderBy: q =>
            {
                var sortBy = option.SortBy!.ToLower();
                var sortDirection = option.SortDirection!.ToLower();
                IOrderedQueryable<Account> orderedQuery = (sortBy, sortDirection) switch
                {
                    ("balance", "asc") => q
                        .OrderBy(x => x.Balance)
                        .ThenBy(x => x.CreatedAt),
                    ("balance", "desc") => q
                        .OrderByDescending(x => x.Balance)
                        .ThenBy(x => x.CreatedAt),
                    ("type", "asc") => q
                        .OrderBy(x => x.AccountType!.Name)
                        .ThenBy(x => x.CreatedAt),
                    ("type", "desc") => q
                        .OrderByDescending(x => x.AccountType!.Name)
                        .ThenBy(x => x.CreatedAt),
                    ("name", "asc") => q
                        .OrderBy(x => x.Tenant!.Name)
                        .ThenBy(x => x.CreatedAt),
                    ("name", "desc") => q
                        .OrderByDescending(x => x.Tenant!.Name)
                        .ThenBy(x => x.CreatedAt),
                    ("lastactivity", "asc") => q
                        .OrderBy(x => x.Transactions.Last().CreatedAt)
                        .ThenBy(x => x.AccountType!.Name),
                    ("lastactivity", "desc") => q
                        .OrderByDescending(x => x.Transactions.Last().CreatedAt)
                        .ThenBy(x => x.AccountType!.Name),
                    _ => q
                        .OrderBy(x => x.CreatedAt)
                        .ThenBy(x => x.AccountType!.Name)
                };
                return orderedQuery;
            },
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