namespace CoreAPI.Repositories.Interfaces;

public interface IRepository<TEntity, in TId> where TEntity : class where TId : class
{
    Task<int> SaveChangeAsync(CancellationToken cancellationToken);
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
}