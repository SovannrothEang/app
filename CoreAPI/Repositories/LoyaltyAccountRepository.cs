using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public interface ILoyaltyAccountRepository
{
    Task<IEnumerable<LoyaltyAccount>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<LoyaltyAccount>> GetAllWithFilteringAsync(
        Expression<Func<LoyaltyAccount, bool>> filtering,
        CancellationToken cancellationToken = default);

    Task<LoyaltyAccount?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<LoyaltyAccount?> GetByTenantAndCustomerWithIncludesAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<LoyaltyAccount>> GetAllWithCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<LoyaltyAccount>> GetAllWithTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}

public class LoyaltyAccountRepository(AppDbContext dbContext) : ILoyaltyAccountRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<LoyaltyAccount>> GetAllAsync(CancellationToken cancellationToken = default) {
        return await _dbContext.LoyaltyAccounts
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyAccount>> GetAllWithFilteringAsync(
        Expression<Func<LoyaltyAccount, bool>> filtering,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyAccounts
            .AsNoTracking()
            .Where(filtering)
            .ToListAsync(cancellationToken);
    }
    public async Task<LoyaltyAccount?> GetByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.CustomerId == customerId, cancellationToken);
    }
    
    public async Task<LoyaltyAccount?> GetByTenantAndCustomerWithIncludesAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyAccounts
            .AsNoTracking()
            .Include(e => e.PointTransactions)
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.CustomerId == customerId, cancellationToken);
    }
    
    public async Task<IEnumerable<LoyaltyAccount>> GetAllWithCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyAccounts
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<LoyaltyAccount>> GetAllWithTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyAccounts
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}