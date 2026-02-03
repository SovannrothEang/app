using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Repositories;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public class Repository<TEntity>(AppDbContext dbContext, IMapper mapper) : IRepository<TEntity> where TEntity : class
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
        queryable = ApplyQueryFilters(queryable, trackChanges, ignoreQueryFilters, filter, includes, orderBy);
        if (select is not null)
            return await queryable
                .Select(select)
                .ToListAsync(cancellationToken);
        
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
        queryable = ApplyQueryFilters(queryable, trackChanges, ignoreQueryFilters, null, includes);
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
    }
    
    public async Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Expression<Func<TEntity, TResult>>? select = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query;
        queryable = queryable.Where(predicate);
        queryable = ApplyQueryFilters(queryable, trackChanges, ignoreQueryFilters, null, includes);
        if (select is not null)
            return await queryable
                .Select(select)
                .FirstOrDefaultAsync(cancellationToken);

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
        queryable = ApplyPaginated(
            queryable,
            ignoreQueryFilters,
            option,
            filter);
        
        if (orderBy is not null)
            queryable = orderBy(queryable);
        var totalCount = await queryable.CountAsync(cancellationToken);
        if (includes is not null)
            queryable = includes(queryable);
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
        queryable = ApplyPaginated(
            queryable,
            ignoreQueryFilters,
            option,
            filter);
        
        if (orderBy is not null)
            queryable = orderBy(queryable);
        var totalCount = await queryable.CountAsync(cancellationToken);
        if (includes is not null)
            queryable = includes(queryable);
        var items = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TResult>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<TResult>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    
    public async Task<decimal> SumAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? filter = null,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query.AsNoTracking();
        if (ignoreQueryFilters)
            queryable = queryable.IgnoreQueryFilters();
        if (filter is not null)
            queryable = queryable.Where(filter);
        return await queryable.SumAsync(selector, cancellationToken);
    }
    
    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }
    
    public async Task CreateBatchAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
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
        if (orderBy is not null)
            queryable = orderBy(queryable);
        if (includes is not null)
            queryable = includes(queryable);
        return queryable;
    }

    private static IQueryable<TEntity> ApplyPaginated(
        IQueryable<TEntity> queryable,
        bool ignoreQueryFilters,
        PaginationOption option,
        Expression<Func<TEntity, bool>>? filter = null)
    {
        if (ignoreQueryFilters)
            queryable = queryable.IgnoreQueryFilters();
        if (filter is not null)
            queryable = queryable.Where(filter);
        var propertyInfo = typeof(TEntity).GetProperty("CreatedAt");

        if (propertyInfo is null ||
            (propertyInfo.PropertyType != typeof(DateTimeOffset) &&
             propertyInfo.PropertyType != typeof(DateTimeOffset?)))
            return queryable;
        
        var parameter = Expression.Parameter(typeof(TEntity), "q");
        var property = Expression.Property(parameter, propertyInfo);
        
        if (option.StartDate.HasValue)
        {
            var startDate = new DateTimeOffset(option.StartDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var constant = Expression.Constant(startDate, typeof(DateTimeOffset));

            Expression comparison;
            if (propertyInfo.PropertyType == typeof(DateTimeOffset?))
            {
                var value = Expression.Property(property, "Value");
                comparison = Expression.GreaterThanOrEqual(value, constant);
                var hasValue = Expression.Property(property, "HasValue");
                comparison = Expression.AndAlso(hasValue, comparison);
            }
            else
            {
                comparison = Expression.GreaterThanOrEqual(property, constant);
            }
            var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
            queryable = queryable.Where(lambda);}
        
        if (option.EndDate.HasValue)
        {
            var endDate = new DateTimeOffset(option.EndDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            var constant = Expression.Constant(endDate, typeof(DateTimeOffset));
                
            Expression comparison;
            if (propertyInfo.PropertyType == typeof(DateTimeOffset?))
            {
                var value = Expression.Property(property, "Value");
                comparison = Expression.LessThanOrEqual(value, constant);
                var hasValue = Expression.Property(property, "HasValue");
                comparison = Expression.AndAlso(hasValue, comparison);
            }
            else
            {
                comparison = Expression.LessThanOrEqual(property, constant);
            }
            var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
            queryable = queryable.Where(lambda);
        }

        return queryable;
    }
}