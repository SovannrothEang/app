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
        if (filtering != null) queryable = queryable.Where(filtering);
        var (result, totalCount) = await GetPaginatedAsync(queryable, option, childIncluded, cancellationToken);
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
        var (result, totalCount) = await GetPaginatedAsync(queryable, pageOption, childIncluded, cancellationToken);
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
        var (result, totalCount) = await GetPaginatedAsync(queryable, option, childIncluded, cancellationToken);
        return (result, totalCount);
    }
    
    #region  Helper Methods
    private static async Task<(IEnumerable<Transaction>, int)> GetPaginatedAsync(
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

        if (!string.IsNullOrEmpty(option.FilterBy) && !string.IsNullOrEmpty(option.FilterValue))
            queryable = option.FilterBy.ToLower() switch
            {
                "id" => queryable
                    .Where(x => x.Id.Equals(option.FilterValue!)),
                "tenantid" => queryable
                    .Where(x => x.TenantId.Equals(option.FilterValue!)),
                "customerid" => queryable
                    .Where(x => x.CustomerId.Equals(option.FilterValue!)),
                "accounttypeid" => queryable
                    .Where(x => x.AccountTypeId.Equals(option.FilterValue!)),
                "type" => queryable
                    .Where(x => x.TransactionType!.Name!.Equals(option.FilterValue!)),
                "occurredat" => DateTime.TryParse(option.FilterValue, out var occurredAt)
                    ? queryable.Where(x => x.OccurredAt.Date == occurredAt.Date)
                    : queryable,
                _ => throw new BadHttpRequestException($"Filtering by '{option.FilterBy}' is not supported.")
            };
        if (option.StartDate is not null)
        {
            var startDate = new DateTimeOffset(option.StartDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryable = queryable.Where(q => q.CreatedAt >= startDate);
        }
        if (option.EndDate is not null)
        {
            var endDate = new DateTimeOffset(option.EndDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            queryable = queryable.Where(q => q.CreatedAt <= endDate);
        }
        
        var totalCount = await queryable.CountAsync(cancellationToken);
        
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
            ("createdat", "asc") => queryable
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.TransactionType!.Name),
            ("createdat", "desc") => queryable
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.TransactionType!.Name),
            _ => queryable
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.TransactionType!.Name)
        };

        var result = await queryable
            .Skip((option.Page!.Value - 1) * option.PageSize!.Value)
            .Take(option.PageSize!.Value)
            .ToListAsync(cancellationToken);
        return (result, totalCount);
    }
    #endregion
}