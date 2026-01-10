using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.DTOs;
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
    /// <param name="option"></param>
    /// <param name="childIncluded"></param>
    /// <param name="filtering"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(IEnumerable<Transaction> result, int totalCount)> GetAllAsync(
        PaginationOption option,
        bool childIncluded = false,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters();

        if (childIncluded)
        {
            queryable = queryable.Include(e => e.TransactionType);
            queryable = queryable.Include(e => e.Customer);
            queryable = queryable.Include(e => e.Performer);
        }
            
        if (filtering != null)
            queryable = queryable.Where(filtering);
                
        var totalCount = await queryable.CountAsync(cancellationToken);
        var trans = await queryable
            .OrderByDescending(e => e.CreatedAt)
            .Skip((option.Page - 1) * option.PageSize)
            .Take(option.PageSize)
            .ToListAsync(cancellationToken);
        return (trans, totalCount);
    }

    public async Task<IEnumerable<Transaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Transactions.AsQueryable();
            
        if (filtering != null)
            queryable = queryable.Where(filtering);
                
        return await queryable
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId && e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetAllByCustomerGlobalAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Transaction>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Transaction>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}