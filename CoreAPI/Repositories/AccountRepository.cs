using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync(
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<Account?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Account>> GetAllWithCustomerAsync(
        string customerId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Account>> GetAllWithTenantAsync(
        string tenantId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);
}

public class AccountRepository(AppDbContext dbContext) : IAccountRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<Account>> GetAllAsync(
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default) 
    {
        var queryable = _dbContext.LoyaltyAccounts.AsQueryable();

        if (childIncluded)
            queryable = queryable.Include(e => e.PointTransactions);
        
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
        var queryable = _dbContext.LoyaltyAccounts.AsQueryable();
        
        if (childIncluded)
            queryable = queryable.Include(e => e.PointTransactions);
        
        return await queryable
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.CustomerId == customerId, cancellationToken);
    }
    
    public async Task<IEnumerable<Account>> GetAllWithCustomerAsync(
        string customerId,
        Expression<Func<Account, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.LoyaltyAccounts.AsQueryable();
        
        if (childIncluded)
            queryable = queryable.Include(e => e.PointTransactions);
        
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
        var queryable = _dbContext.LoyaltyAccounts.AsQueryable();
        
        if (childIncluded)
            queryable = queryable.Include(e => e.PointTransactions);
        
        if (filtering != null)
            queryable = queryable.Where(filtering);
        
        return await queryable
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}