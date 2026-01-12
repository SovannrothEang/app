using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class UserRepository(AppDbContext dbContext, ICurrentUserProvider currentUserProvider) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;

    // Assign User within the current tenant
    public async Task AddToRoleAsync(string userId, string roleId, string tenantId)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            TenantId = tenantId
        };

        await _dbContext.UserRoles.AddAsync(userRole);
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(r => r.UserId == userId)
            .Where(r => r.Role != null && r.Role.Name != null)
            .Select(r => r.Role!.Name!)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(
        Expression<Func<User, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Users
            .AsQueryable()
            .IgnoreQueryFilters();

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