using System.Linq.Expressions;
using CoreAPI.DTOs;

namespace CoreAPI.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// List all entities
    /// </summary>
    /// <param name="trackChanges">If true, the entities will be tracked by the context.</param>
    /// <param name="ignoreQueryFilters">If true, the global query filters will be ignored.</param>
    /// <param name="includes">The includes to be applied to the query.</param>
    /// <param name="filter">The filter to be applied to the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of entities.</returns>
    Task<IEnumerable<TEntity>> ListAsync(
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<TResult>> ListAsync<TResult>(
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        CancellationToken cancellationToken = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        bool trackChanges = false,
        bool ignoreQueryFilters = false,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<TEntity> items, int totalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        CancellationToken cancellationToken = default);
    Task<PagedResult<TResult>> GetPagedAsync<TResult>(
        int page,
        int pageSize,
        bool ignoreQueryFilters = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        CancellationToken cancellationToken = default);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    IQueryable<TEntity> Query();
}