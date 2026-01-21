using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CoreAPI.Data;
using CoreAPI.DTOs;
using CoreAPI.Models.Shared;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class Repository<TEntity>(AppDbContext dbContext, IMapper mapper) : IRepository<TEntity> where TEntity : class, IAuditEntity
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TEntity>> ListAsync(
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query;
        queryable = ApplyQueryFilters(queryable, trackChanges, ignoreQueryFilters, filter, includes, orderBy);
        return await queryable.ToListAsync(cancellationToken);
    }
    public async Task<IEnumerable<TResult>> ListAsync<TResult>(
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, TResult>>? select = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query;
            queryable = queryable.AsNoTracking();
        queryable = ApplyQueryFilters(queryable, trackChanges, ignoreQueryFilters, filter, includes, orderBy);
        if (select is not null)
            return await queryable
                .Select(select)
                .ToListAsync(cancellationToken);
        else
            return await queryable
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
    }
    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query;
        queryable = ApplyQueryFilters(queryable, trackChanges, ignoreQueryFilters, null, includes, null);
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
    }
    public async Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query;
        queryable = queryable.Where(predicate);
        queryable = ApplyQueryFilters(queryable, trackChanges, ignoreQueryFilters, null, includes, null);
        return await queryable
            .ProjectTo<TResult>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query;
        if (ignoreQueryFilters)
            queryable = queryable.IgnoreQueryFilters();
        return await queryable.AnyAsync(predicate, cancellationToken);
    }
    public async Task<(IEnumerable<TEntity> items, int totalCount)> GetPagedResultAsync(
        PaginationOption option,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var page = option.Page.GetValueOrDefault(1);
        var pageSize = option.PageSize.GetValueOrDefault(10);
        var queryable = Query.AsNoTracking();
        if (ignoreQueryFilters)
            queryable = queryable.IgnoreQueryFilters();
        if (filter is not null)
            queryable = queryable.Where(filter);
        if (option.StartDate.HasValue)
        {
            var startDate = new DateTimeOffset(option.StartDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryable = queryable.Where(q => q.CreatedAt >= startDate);
        }
        if (option.EndDate.HasValue)
        {
            var endDate = new DateTimeOffset(option.EndDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            queryable = queryable.Where(q => q.CreatedAt <= endDate);
        }
        var totalCount = await queryable.CountAsync(cancellationToken);
        if (includes is not null)
            queryable = includes(queryable);
        if (orderBy is not null)
            queryable = orderBy(queryable);
        var items = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }
    public async Task<PagedResult<TResult>> GetPagedResultAsync<TResult>(
        PaginationOption option,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var page = option.Page.GetValueOrDefault(1);
        var pageSize = option.PageSize.GetValueOrDefault(10);
        var queryable = Query.AsNoTracking();
        if (ignoreQueryFilters)
            queryable = queryable.IgnoreQueryFilters();
        if (filter is not null)
            queryable = queryable.Where(filter);
        if (option.StartDate.HasValue)
        {
            var startDate = new DateTimeOffset(option.StartDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            queryable = queryable.Where(q => q.CreatedAt >= startDate);
        }
        if (option.EndDate.HasValue)
        {
            var endDate = new DateTimeOffset(option.EndDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            queryable = queryable.Where(q => q.CreatedAt <= endDate);
        }
        var totalCount = await queryable.CountAsync(cancellationToken);
        if (includes is not null)
            queryable = includes(queryable);
        if (orderBy is not null)
            queryable = orderBy(queryable);
        var items = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TResult>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return new PagedResult<TResult>
        {
            Items = items ?? [],
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }
    public void Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
    }
    public void Remove(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }
    private IQueryable<TEntity> Query => _dbContext.Set<TEntity>();
    private static IQueryable<TEntity> ApplyQueryFilters(
        IQueryable<TEntity> queryable,
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        if (!trackChanges)
            queryable = queryable.AsNoTracking();
        if (ignoreQueryFilters)
            queryable = queryable.IgnoreQueryFilters();
        if (filter is not null)
            queryable = queryable.Where(filter);
        if (includes is not null)
            queryable = includes(queryable);
        if (orderBy is not null)
            queryable = orderBy(queryable);
        return queryable;
    }
}