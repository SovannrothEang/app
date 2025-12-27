using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task AddToRoleAsync(string userId, string roleId)
    {
        var userRole = new IdentityUserRole<string>
        {
            UserId = userId,
            RoleId = roleId
        };
        
        await _dbContext.UserRoles.AddAsync(userRole);
    }

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

    public async Task CreateAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(entity, cancellationToken);
    }

    public Task Update(User entity)
    {
        _dbContext.Users.Update(entity);
        return Task.CompletedTask;
    }

    public Task Remove(User user)
    {
        _dbContext.Users.Remove(user);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}