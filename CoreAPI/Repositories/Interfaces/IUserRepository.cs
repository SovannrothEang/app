using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>>? filtering = null,
        CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task Update(string id, User user, CancellationToken cancellationToken = default);
    void Remove(User user);
    Task<int?> SaveChanges(CancellationToken cancellationToken = default);
}

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<User>> GetAllAsync(
        Expression<Func<User, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Users.AsQueryable();
            
        if (filtering != null)
            queryable = queryable.Where(filtering);
            
        return await queryable
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task Update(string id, User user, CancellationToken cancellationToken = default)
    {
        var exist  = await _dbContext.Users.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException();
        
        _dbContext.Users.Update(user);
    }

    public void Remove(User user)
    {
        _dbContext.Set<User>().Remove(user);
    }

    public async Task<int?> SaveChanges(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}