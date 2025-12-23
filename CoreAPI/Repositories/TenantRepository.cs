using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class TenantRepository(AppDbContext dbContext) : Repository<Tenant, string>(dbContext), ITenantRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetAllWithFiltering(Expression<Func<Tenant, bool>> filtering, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .Where(filtering)
            .ToListAsync(cancellationToken);
    }
}