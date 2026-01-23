using System.Linq.Expressions;
using CoreAPI.DTOs;

namespace CoreAPI.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Get a List of entity type of TEntity
    /// </summary>
    /// <param name="trackChanges">If true, the entities will be tracked by the context.</param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored.</param>
    /// <param name="filter">The filter to be applied to the query.</param>
    /// <param name="includes">The includes to be applied to the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<TEntity>> ListAsync(
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a List of entity type of TEntity, then project it to TResult 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="trackChanges">If true, the entities will be tracked by the context.</param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored.</param>
    /// <param name="filter">The filter to be applied to the query.</param>
    /// <param name="includes">The includes to be applied to the query.</param>
    /// <param name="select">The select to be applied to the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<TResult>> ListAsync<TResult>(
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, TResult>>? select = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an entity type of TEntity with predicate expression
    /// </summary>
    /// <param name="predicate">An expression to find the entity</param>
    /// <param name="trackChanges">If true, the entities will be tracked by the context.</param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored.</param>
    /// <param name="includes">The includes to be applied to the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an entity type of TEntity, then project to TResult
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="predicate">An expression to find the entity</param>
    /// <param name="trackChanges">If true, the entities will be tracked by the context.</param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored.</param>
    /// <param name="includes">The includes to be applied to the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a boolean value that indicate whether the entity type of TEntity is exist or not
    /// </summary>
    /// <param name="predicate">An expression to find the entity</param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get paged result of the entity type of TEntity
    /// </summary>
    /// <param name="option"></param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored.</param>
    /// <param name="filter">The filter to be applied to the query.</param>
    /// <param name="includes">The includes to be applied to the query.</param>
    /// <param name="orderBy">The order to be applied to the query</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<(IEnumerable<TEntity> items, int totalCount)> GetPagedResultAsync(
        PaginationOption option,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <summary>
    /// Get paged result of the entity type of TEntity
    /// </summary>
    /// <param name="option"></param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored.</param>
    /// <param name="filter">The filter to be applied to the query.</param>
    /// <param name="includes">The includes to be applied to the query.</param>
    /// <param name="orderBy">The order to be applied to the query</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<PagedResult<TResult>> GetPagedResultAsync<TResult>(
        PaginationOption option,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate sum of a numeric property on the database side
    /// </summary>
    /// <param name="selector">The property to sum</param>
    /// <param name="filter">Optional filter to be applied to the query</param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task<decimal> SumAsync(
        Expression<Func<TEntity, decimal>> selector,
        Expression<Func<TEntity, bool>>? filter = null,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default);

    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task CreateBatchAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}