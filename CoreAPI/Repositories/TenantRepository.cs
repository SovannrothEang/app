using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class TenantRepository(AppDbContext dbContext) : ITenantRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<Tenant>> GetAllAsync(
        Expression<Func<Tenant, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Tenants
            .AsNoTracking()
            .AsQueryable();
        
        if (filtering != null)
            queryable = queryable.Where(filtering);
            
        return await queryable
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Tenants
            .AsQueryable();
        
        return await queryable.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> IsExistByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .AnyAsync(e => e.Name == name, cancellationToken);
    }
    
    public async Task<bool> IsExistByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants.AnyAsync(e => e.Id == id, cancellationToken);
    }
}