using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class TenantRepository(AppDbContext dbContext) : Repository<Tenant>(dbContext), ITenantRepository
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
            .AsNoTracking()
            .AsQueryable();
        
        return await queryable
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}