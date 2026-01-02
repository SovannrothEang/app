using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class CustomerRepository(AppDbContext dbContext) : Repository<Customer>(dbContext), ICustomerRepository
{
    private readonly AppDbContext _dbContext = dbContext;

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
    
    // TODO: Fix this logic and return the customer that has relationship with tenant
    // Customer is belong to the Host tenant, but to get the scope tenant, we have to go through the accounts
    public async Task<IEnumerable<Customer>> GetAllCustomersPerTenantAsync(
        bool childIncluded = false,
        Expression<Func<Customer, bool>>? filtering = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Customers
            .AsQueryable();
        
        queryable = queryable.Where(e => e.Accounts.Any());
        
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

    public async Task<Customer?> GetByIdAsync(string id, bool childIncluded = false, CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Customers
            .AsQueryable()
            .IgnoreQueryFilters();

        if (childIncluded)
            queryable = queryable
                .Include(e => e.Accounts)
                .ThenInclude(e => e.Transactions);
        
        return await queryable
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
    
    public async Task<Customer?> GetByIdInTenantScopeAsync(
        string id,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.Customers
            .AsQueryable();

        if (childIncluded)
            queryable = queryable
                .Include(e => e.Accounts)
                .ThenInclude(e => e.Transactions);
        
        return await queryable
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}