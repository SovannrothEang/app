namespace CoreAPI.Repositories.Interfaces;

public interface IRepository<in TEntity> where TEntity : class
{
    Task<int> SaveChangeAsync(CancellationToken cancellationToken);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task Update(TEntity entity);
    Task Remove(TEntity entity);
}