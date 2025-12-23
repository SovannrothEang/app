using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Models.Shared;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public abstract class Repository<TEntity, TId>(AppDbContext dbContext)
    where TEntity : BaseEntity
    where TId : class
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .FindAsync([id], cancellationToken);
    }

    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}