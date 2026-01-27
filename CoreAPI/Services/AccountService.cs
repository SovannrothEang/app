using System.Linq.Expressions;
using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Accounts;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        var (accounts, totalCount) = await _repository.GetPagedResultAsync(
            option: option,
            ignoreQueryFilters: true,
            filter: BuildFilter(customerId, option),
            includes: BuildIncludes(),
            orderBy: BuildOrderBy(option),
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

        var totalBalance = await _repository.SumAsync(
            selector: a => a.Balance,
            filter: a => a.CustomerId == customerId,
            cancellationToken: ct);
        return totalBalance;
    }

    #region Helper Methods
    
    /// <summary>
    /// Builds filter expression for account queries.
    /// Supports: tenantid, name (tenant name), typeid, type (account type name).
    /// </summary>
    private static Expression<Func<Account, bool>> BuildFilter(string customerId, PaginationOption option)
    {
        if (string.IsNullOrEmpty(option.FilterBy) || string.IsNullOrEmpty(option.FilterValue))
            return a => a.CustomerId == customerId;

        return option.FilterBy.ToLower() switch
        {
            "tenantid" => a => a.CustomerId == customerId && a.TenantId == option.FilterValue,
            "name" => a => a.CustomerId == customerId && a.Tenant!.Name == option.FilterValue,
            "typeid" => a => a.CustomerId == customerId && a.AccountTypeId == option.FilterValue,
            "type" => a => a.CustomerId == customerId && a.AccountType!.Name == option.FilterValue,
            _ => a => a.CustomerId == customerId
        };
    }

    /// <summary>
    /// Builds `includes` for account queries.
    /// Includes AccountType, Customer, Tenant, Performer, and last Transaction.
    /// </summary>
    private static Func<IQueryable<Account>, IQueryable<Account>> BuildIncludes()
    {
        return queryable => queryable
            .Include(e => e.AccountType)
            .Include(e => e.Customer)
            .Include(e => e.Tenant)
            .Include(e => e.Performer)
            .Include(e => e.Transactions
                .OrderByDescending(x => x.CreatedAt)
                .Take(1));
    }

    /// <summary>
    /// Builds order by expression for account queries.
    /// Supports: balance, type, name (tenant name), lastactivity, createdat (default).
    /// </summary>
    private static Func<IQueryable<Account>, IOrderedQueryable<Account>> BuildOrderBy(PaginationOption option)
    {
        var sortBy = (option.SortBy ?? "createdAt").ToLower();
        var sortDirection = (option.SortDirection ?? "asc").ToLower();

        return (sortBy, sortDirection) switch
        {
            ("balance", "asc") => q => q
                .OrderBy(x => x.Balance)
                .ThenBy(x => x.CreatedAt),
            ("balance", "desc") => q => q
                .OrderByDescending(x => x.Balance)
                .ThenBy(x => x.CreatedAt),
            ("type", "asc") => q => q
                .OrderBy(x => x.AccountType!.Name)
                .ThenBy(x => x.CreatedAt),
            ("type", "desc") => q => q
                .OrderByDescending(x => x.AccountType!.Name)
                .ThenBy(x => x.CreatedAt),
            ("name", "asc") => q => q
                .OrderBy(x => x.Tenant!.Name)
                .ThenBy(x => x.CreatedAt),
            ("name", "desc") => q => q
                .OrderByDescending(x => x.Tenant!.Name)
                .ThenBy(x => x.CreatedAt),
            ("lastactivity", "asc") => q => q
                .OrderBy(x => x.Transactions.Max(t => t.CreatedAt))
                .ThenBy(x => x.AccountType!.Name),
            ("lastactivity", "desc") => q => q
                .OrderByDescending(x => x.Transactions.Max(t => t.CreatedAt))
                .ThenBy(x => x.AccountType!.Name),
            _ => q => q
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.AccountType!.Name)
        };
    }
    
    #endregion
}
