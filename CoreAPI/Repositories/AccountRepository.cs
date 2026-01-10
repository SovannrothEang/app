using System.Linq.Expressions;
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
        var totalTransaction = await queryTransaction.CountAsync(cancellationToken);

        // TODO: take more time to look into this, some case might be error
        if (childIncluded)
        {
            queryTransaction = queryTransaction
                .Include(e => e.TransactionType)
                .Include(e => e.Customer)
                .Include(e => e.Performer)
                .Where(e => e.TenantId == tenantId && e.CustomerId == customerId);
            // queryable = queryable.Include(e => e.Performer);
        }
        
        var account = await queryable
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.CustomerId == customerId, cancellationToken);
        var transaction = await queryTransaction
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageOption.Page - 1) * pageOption.PageSize)
            .Take(pageOption.PageSize)
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
        var queryable = _dbContext.Accounts.AsQueryable();
        
        if (childIncluded)
            queryable = queryable.Include(e => e.Transactions);
        
        if (filtering != null)
            queryable = queryable.Where(filtering);
        
        return await queryable
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId)
            .ToListAsync(cancellationToken);
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