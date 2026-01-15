using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.DTOs;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class AccountRepository(
    AppDbContext dbContext,
    ITransactionRepository transactionRepository,
    ILogger<AccountRepository> logger)
    : IAccountRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ILogger<AccountRepository> _logger = logger;

    // Get all, but it attached to TenantID (Global query) 
    public async Task<(IEnumerable<Account> result, int totalCount)> GetAllAsync(
        PaginationOption option,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts
            .AsNoTracking()
            .AsQueryable();
        var totalCount = await queryable.CountAsync(cancellationToken);
        queryable = queryable.Include(e => e.AccountType);
        if (childIncluded)
            queryable = queryable
                .Include(e => e.Tenant)
                .Include(e => e.Customer)
                .Include(e => e.Performer);
        if (filtering != null) queryable = queryable.Where(filtering);
        var result = await GetPaginatedAsync(queryable, option, cancellationToken);
        return (result, totalCount);
    }
    
    public async Task<IEnumerable<Account>> GetAllByCustomerIdAsync(
        string customerId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts
            .AsNoTracking()
            .AsQueryable()
            .Where(e => e.CustomerId == customerId);
        queryable = queryable.Include(e => e.AccountType);
        if (childIncluded)
            queryable = queryable
                .Include(e => e.Tenant)
                .Include(e => e.Customer)
                .Include(e => e.Performer);
        if (filtering != null) queryable = queryable.Where(filtering);
        return await queryable.ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Account> result, int totalCount)> GetAllByCustomerIdForGlobalAsync(
        string customerId,
        PaginationOption option,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters() // Customer only
            .Where(e => e.CustomerId == customerId);
        var totalCount = await queryable.CountAsync(cancellationToken);
        //if (childIncluded)
            queryable = queryable
                .Include(e => e.AccountType)
                .Include(e => e.Customer)
                .Include(e => e.Tenant)
                .Include(e => e.Performer);
        // If there are no transaction included, we'll just throw the last activity into it
        queryable = childIncluded
            ? queryable.Include(e => e.Transactions)
            : queryable.Include(e => e.Transactions
                .OrderByDescending(x => x.CreatedAt)
                .Take(1));
        if (filtering != null) queryable = queryable.Where(filtering);
        var result = await GetPaginatedAsync(queryable, option, cancellationToken);
        return (result, totalCount);
    }

    #region Helper Methods
    private static async Task<IEnumerable<Account>> GetPaginatedAsync(
        IQueryable<Account> queryable,
        PaginationOption pageOption,
        CancellationToken cancellationToken = default)
    {
        if (pageOption.StartDate.HasValue)
        {
            var startDate = new DateTimeOffset(pageOption.StartDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryable = queryable.Where(x => x.CreatedAt >= startDate);
        }
        if (pageOption.EndDate.HasValue)
        {
            var endDate = new DateTimeOffset(pageOption.EndDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryable = queryable.Where(x => x.CreatedAt <= endDate);
        }
        
        var sortBy = pageOption.SortBy!.ToLower();
        var sortDirection = pageOption.SortDirection!.ToLower();
        queryable = (sortBy, sortDirection) switch
        {
            ("balance", "asc") => queryable
                .OrderBy(x => x.Balance)
                .ThenBy(x => x.CreatedAt),
            ("balance", "desc") => queryable
                .OrderByDescending(x => x.Balance)
                .ThenBy(x => x.CreatedAt),
            ("type", "asc") => queryable
                .OrderBy(x => x.AccountType!.Name)
                .ThenBy(x => x.CreatedAt),
            ("type", "desc") => queryable
                .OrderByDescending(x => x.AccountType!.Name)
                .ThenBy(x => x.CreatedAt),
            ("name", "asc") => queryable
                .OrderBy(x => x.Tenant!.Name)
                .ThenBy(x => x.CreatedAt),
            ("name", "desc") => queryable
                .OrderByDescending(x => x.Tenant!.Name)
                .ThenBy(x => x.CreatedAt),
            ("lastactivity", "asc") => queryable
                .OrderBy(x => x.Transactions.Last().CreatedAt)
                .ThenBy(x => x.AccountType!.Name),
            ("lastactivity", "desc") => queryable
                .OrderByDescending(x => x.Transactions.Last().CreatedAt)
                .ThenBy(x => x.AccountType!.Name),
            _ => queryable
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.AccountType!.Name)
        };
        return await queryable
            .Skip((pageOption.Page!.Value - 1) * pageOption.PageSize!.Value)
            .Take(pageOption.PageSize!.Value)
            .ToListAsync(cancellationToken);
    }
    #endregion
}