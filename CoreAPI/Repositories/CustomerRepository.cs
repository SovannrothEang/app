using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class CustomerRepository(AppDbContext dbContext, ICurrentUserProvider currentUserProvider) : Repository<Customer>(dbContext), ICustomerRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;

    public async Task<IEnumerable<Customer>> GetAllAsync(
        bool childIncluded = false,
        Expression<Func<Customer, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Customers
            .AsQueryable()
            .IgnoreQueryFilters();

        queryable = queryable.Include(e => e.User);

        if (childIncluded)
            queryable = queryable
                .Include(e => e.Accounts)
                .ThenInclude(e => e.Transactions);

        if (filtering != null)
            queryable = queryable.Where(filtering);

        return await queryable
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // Customer is belong to the Host tenant, but to get the scope tenant, we have to go through the accounts
    public async Task<IEnumerable<Customer>> GetAllCustomersPerTenantAsync(
        bool childIncluded = false,
        Expression<Func<Customer, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Customers
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters()
            .Where(e => e.Accounts.Any(a => a.TenantId == _currentUserProvider.TenantId));

        queryable = queryable.Include(e => e.User);
        if (childIncluded)
            queryable = queryable
                .Include(e => e.Accounts.Where(a
                    => a.TenantId == _currentUserProvider.TenantId));
                // .ThenInclude(e => e.Transactions);

        if (filtering != null)
            queryable = queryable.Where(filtering);

        return await queryable.ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(string id, bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Customers
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters();
        
        queryable = queryable.Include(e => e.User);
        if (childIncluded)
            queryable = queryable
                .Include(e => e.Accounts)
                .ThenInclude(e => e.Transactions);

        return await queryable.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByIdInTenantScopeAsync(
        string id,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Customers
            .AsNoTracking()
            .AsQueryable()
            .IgnoreQueryFilters()
            .Where(e  => e.Id == id)
            .Where(e => e.Accounts.Any(a
                => a.TenantId == _currentUserProvider.TenantId));

        queryable = queryable.Include(e => e.User);
        if (childIncluded)
            queryable = queryable
                .Include(e => e.Accounts
                    .Where(a => a.TenantId == _currentUserProvider.TenantId));
            // .ThenInclude(e => e.Transactions);

        return await queryable.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithinTenantScopeAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsQueryable()
            .AnyAsync(e => e.Id == id, cancellationToken);
    }

}