using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.DTOs;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            .Skip((option.Page!.Value - 1) * option.PageSize!.Value)
            .Take(option.PageSize!.Value)
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

    public async Task<(IEnumerable<Transaction> list, int totalCount)> GetAllByCustomerGlobalAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded,
        CancellationToken cancellationToken = default)
    {
        var queryable =  _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters()
            .Where(e => e.CustomerId == customerId);

        queryable = queryable.Include(e => e.TransactionType);

        if (childIncluded)
            queryable = queryable
                .Include(e => e.Customer)
                .Include(e => e.Performer)
                .Where(e => e.CustomerId == customerId);
        
        
        if (!string.IsNullOrEmpty(pageOption.TransactionType))
        {
            queryable = queryable.Where(x => x.TransactionType!.Slug == pageOption.TransactionType);
        }
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
        
        var totalTransaction = await queryable.CountAsync(cancellationToken);

        var sortBy = pageOption.SortBy!.ToLower();
        var sortDirection = pageOption.SortDirection!.ToLower();
        queryable = (sortBy, sortDirection) switch
        {
            ("balance", "asc") => queryable.OrderBy(x => x.Amount),
            ("balance", "desc") => queryable.OrderByDescending(x => x.Amount),
            ("type", "asc") => queryable.OrderBy(x => x.TransactionType),
            ("type", "desc") => queryable.OrderByDescending(x => x.TransactionType),
            ("occurredat", "asc") => queryable.OrderBy(x => x.OccurredAt),
            ("occurredat", "desc") => queryable.OrderByDescending(x => x.OccurredAt),
            _ => queryable.OrderBy(x => x.CreatedAt)
        };

        var result = await queryable
            .Skip((pageOption.Page!.Value - 1) * pageOption.PageSize!.Value)
            .Take(pageOption.PageSize!.Value)
            .ToListAsync(cancellationToken);
        
        return (result, totalTransaction);
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