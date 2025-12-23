using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Models.Shared;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public abstract class Repository<TEntity>(AppDbContext dbContext)
    where TEntity : BaseEntity
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public Task Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }
    
    public Task Remove(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}