using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class PointTransactionRepository(AppDbContext dbContext) : IPointTransactionRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<PointTransaction>> GetAllAsync(Expression<Func<PointTransaction, bool>>? filtering = null, CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.PointTransactions.AsQueryable();
            
        if (filtering != null)
            queryable = queryable.Where(filtering);
                
        return await queryable
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PointTransaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        Expression<Func<PointTransaction, bool>>? filtering = null,
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

    public async Task<PointTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PointTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}