using System.Linq.Expressions;
using Azure;
using CoreAPI.Data;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class AccountRepository(AppDbContext dbContext) : IAccountRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<Account>> GetAllAsync(
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts.AsQueryable();

        if (childIncluded)
            queryable = queryable.Include(e => e.Transactions);

        if (filtering != null)
            queryable = queryable.Where(filtering);

        return await queryable
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts.AsQueryable();

        if (childIncluded)
        {
            queryable = queryable.Include(e => e.Transactions);
            queryable = queryable.Include("Transactions.TransactionType");
            queryable = queryable.Include("Transactions.Customer");
            queryable = queryable.Include("Transactions.Performer");
            // queryable = queryable.Include(e => e.Performer);
        }

        return await queryable
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.CustomerId == customerId, cancellationToken);
    }

    public async Task<(Account? account, IEnumerable<Transaction> transactions, int totalTransaction)> GetByTenantAndCustomerPaginationAsync(
        string tenantId,
        string customerId,
        CustomerGetBalanceOptionsDto? option,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts.AsQueryable();
        var queryTransaction = _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable();

        queryTransaction = queryTransaction.Include(e => e.TransactionType);
        if (childIncluded)
        {
            queryTransaction = queryTransaction
                .Include(e => e.Customer)
                .Include(e => e.Performer)
                .Where(e => e.TenantId == tenantId && e.CustomerId == customerId);
        }

        var account = await queryable
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.CustomerId == customerId, cancellationToken);
        
        if (!string.IsNullOrEmpty(pageOption.TransactionType))
        {
            queryTransaction = queryTransaction.Where(x => x.TransactionType!.Slug == pageOption.TransactionType);
        }
        if (pageOption.StartDate.HasValue)
        {
            var startDate = new DateTimeOffset(pageOption.StartDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryTransaction = queryTransaction.Where(x => x.CreatedAt >= startDate);
        }
        if (pageOption.EndDate.HasValue)
        {
            var endDate = new DateTimeOffset(pageOption.EndDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryTransaction = queryTransaction.Where(x => x.CreatedAt <= endDate);
        }
        
        var totalTransaction = await queryTransaction.CountAsync(cancellationToken);

        var sortBy = pageOption.SortBy!.ToLower();
        var sortDirection = pageOption.SortDirection!.ToLower();
        queryTransaction = (sortBy, sortDirection) switch
        {
            ("balance", "asc") => queryTransaction.OrderBy(x => x.Amount),
            ("balance", "desc") => queryTransaction.OrderByDescending(x => x.Amount),
            ("type", "asc") => queryTransaction.OrderBy(x => x.TransactionType),
            ("type", "desc") => queryTransaction.OrderByDescending(x => x.TransactionType),
            ("occurredat", "asc") => queryTransaction.OrderBy(x => x.OccurredAt),
            ("occurredat", "desc") => queryTransaction.OrderByDescending(x => x.OccurredAt),
            _ => queryTransaction.OrderBy(x => x.CreatedAt)
        };

        // Null-forgiven since we init the value even if the user don't assign it anything
        var transaction = await queryTransaction
            .Skip((pageOption.Page!.Value - 1) * pageOption.PageSize!.Value)
            .Take(pageOption.PageSize!.Value)
            .ToListAsync(cancellationToken);

        if (account is null)
            return (null, [], 0);
        return (account, transaction, totalTransaction);
    }

    public async Task<IEnumerable<Account>> GetAllWithCustomerAsync(
        string customerId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters() // Customer only
            .Where(e => e.CustomerId == customerId);

        queryable = queryable.Include(e => e.Tenant);
        if (childIncluded)
        {
            // I want to get only the latest transaction
            queryable = queryable.Include(e => e.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(1));
        }

        if (filtering != null)
            queryable = queryable.Where(filtering);

        return await queryable.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAllWithTenantAsync(
        string tenantId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts.AsQueryable();

        if (childIncluded)
            queryable = queryable.Include(e => e.Transactions);

        if (filtering != null)
            queryable = queryable.Where(filtering);

        return await queryable
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}