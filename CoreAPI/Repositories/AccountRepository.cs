using System.Linq.Expressions;
using Azure;
using CoreAPI.Data;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class AccountRepository(AppDbContext dbContext, ITransactionRepository transactionRepository)
    : IAccountRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

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
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Accounts
            .AsQueryable();
        // I don't IgnoreQueryGlobal cuz I want to make sure that Tenant is gonna be getting what their
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
        
        var (result, totalCount) = await _transactionRepository
            .GetPaginatedAsync(queryTransaction, pageOption, cancellationToken);

        if (account is null)
            return (null, [], 0);
        return (account, result, totalCount);
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