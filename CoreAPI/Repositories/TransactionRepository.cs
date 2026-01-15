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

    public async Task<(IEnumerable<Transaction> result, int totalCount)> GetAllForGlobalAsync(
        PaginationOption option,
        bool childIncluded = false,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters();
        var totalCount = await queryable.CountAsync(cancellationToken);
        if (filtering != null) queryable = queryable.Where(filtering);
        var result = await this.GetPaginatedAsync(queryable, option, childIncluded, cancellationToken);
        return (result, totalCount);
    }

    public async Task<IEnumerable<Transaction>> GetAllByTenantAsync(
        string tenantId,
        string customerId,
        bool childIncluded,
        Expression<Func<Transaction, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable()
            .Where(e => e.CustomerId == customerId)
            .Where(e => e.Account!.TenantId == tenantId);
        queryable = queryable
            .Include(e => e.Account)
            .Include(e => e.TransactionType);
        if (childIncluded)
            queryable = queryable
                .Include(e => e.Referencer)
                .Include(e => e.Performer);
        if (filtering != null) queryable = queryable.Where(filtering);
        return await queryable.ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Transaction> list, int totalCount)>
        GetAllByCustomerIdAsync(
            string customerId,
            PaginationOption pageOption,
            bool childIncluded,
            CancellationToken cancellationToken = default)
    {
        var queryable =  _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable()
            .Where(e => e.CustomerId == customerId);
        var totalCount = await queryable.CountAsync(cancellationToken);
        var result = await this.GetPaginatedAsync(queryable, pageOption, childIncluded, cancellationToken);
        return (result, totalCount);
    }

    public async Task<(IEnumerable<Transaction> list, int totalCount)>
        GetAllByCustomerIdForGlobalAsync(
            string customerId,
            PaginationOption option,
            bool childIncluded,
            CancellationToken cancellationToken = default)
    {
        var queryable =  _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters()
            .Where(e => e.CustomerId == customerId);
        var totalCount = await queryable.CountAsync(cancellationToken);
        var result = await this.GetPaginatedAsync(queryable, option, childIncluded, cancellationToken);
        return (result, totalCount);
    }
    
    #region  Helper Methods
    private async Task<IEnumerable<Transaction>> GetPaginatedAsync(
        IQueryable<Transaction> queryable,
        PaginationOption option,
        bool childIncluded,
        CancellationToken cancellationToken = default)
    {
        queryable = queryable.Include(e => e.TransactionType);
        if (childIncluded)
            queryable = queryable
                .Include(e => e.Referencer)
                .Include(e => e.Performer);

        if (!string.IsNullOrEmpty(option.TransactionType))
        {
            queryable = queryable.Where(x => x.TransactionType!.Slug == option.TransactionType);
        }
        if (option.StartDate.HasValue)
        {
            var startDate = new DateTimeOffset(option.StartDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryable = queryable.Where(x => x.CreatedAt >= startDate);
        }
        if (option.EndDate.HasValue)
        {
            var endDate = new DateTimeOffset(option.EndDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryable = queryable.Where(x => x.CreatedAt <= endDate);
        }
        
        var sortBy = option.SortBy!.ToLower();
        var sortDirection = option.SortDirection!.ToLower();
        queryable = (sortBy, sortDirection) switch
        {
            ("balance", "asc") => queryable.OrderBy(x => x.Amount),
            ("balance", "desc") => queryable.OrderByDescending(x => x.Amount),
            ("type", "asc") => queryable
                .OrderBy(x => x.TransactionType!.Name)
                .ThenBy(x => x.OccurredAt),
            ("type", "desc") => queryable
                .OrderByDescending(x => x.TransactionType!.Name)
                .ThenBy(x => x.OccurredAt),
            ("occurredat", "asc") => queryable
                .OrderBy(x => x.OccurredAt)
                .ThenBy(x => x.TransactionType!.Name),
            ("occurredat", "desc") => queryable
                .OrderByDescending(x => x.OccurredAt)
                .ThenBy(x => x.TransactionType!.Name),
            _ => queryable
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.TransactionType!.Name)
        };

        return await queryable
            .Skip((option.Page!.Value - 1) * option.PageSize!.Value)
            .Take(option.PageSize!.Value)
            .ToListAsync(cancellationToken);
    }
    #endregion
}