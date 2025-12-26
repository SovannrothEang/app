using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public interface ILoyaltyAccountRepository
{
    Task<IEnumerable<LoyaltyAccount>> GetAllAsync(
        Expression<Func<LoyaltyAccount, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<LoyaltyAccount?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<LoyaltyAccount>> GetAllWithCustomerAsync(
        string customerId,
        Expression<Func<LoyaltyAccount, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<LoyaltyAccount>> GetAllWithTenantAsync(
        string tenantId,
        Expression<Func<LoyaltyAccount, bool>>? filtering = null,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);
}

public class LoyaltyAccountRepository(AppDbContext dbContext) : ILoyaltyAccountRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<LoyaltyAccount>> GetAllAsync(
        Expression<Func<LoyaltyAccount, bool>>? filtering = null,
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

    public async Task<LoyaltyAccount?> GetByTenantAndCustomerAsync(
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
    
    public async Task<IEnumerable<LoyaltyAccount>> GetAllWithCustomerAsync(
        string customerId,
        Expression<Func<LoyaltyAccount, bool>>? filtering = null,
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
    
    public async Task<IEnumerable<LoyaltyAccount>> GetAllWithTenantAsync(
        string tenantId,
        Expression<Func<LoyaltyAccount, bool>>? filtering = null,
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