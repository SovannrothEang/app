using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class TransactionRepository(AppDbContext dbContext) : ITransactionRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// This is for the SuperAdminGetting the Overall transactions
    /// </summary>
    /// <param name="filtering"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Transaction>> GetAllAsync(Expression<Func<Transaction, bool>>? filtering = null, CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.PointTransactions
            .AsQueryable()
            .IgnoreQueryFilters();
            
        if (filtering != null)
            queryable = queryable.Where(filtering);
                
        return await queryable
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.PointTransactions.AsQueryable();
            
        if (filtering != null)
            queryable = queryable.Where(filtering);
                
        return await queryable
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId && e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetAllByCustomerGlobalAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PointTransactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PointTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Transaction>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PointTransactions
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Transaction>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PointTransactions
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}