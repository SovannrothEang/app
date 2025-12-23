using System.Linq.Expressions;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class CustomerRepository(AppDbContext dbContext) : Repository<Customer, string>(dbContext), ICustomerRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    
    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Include(e => e.LoyaltyAccounts)
            .ThenInclude(e => e.PointTransactions)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetAllWithFiltering(Expression<Func<Customer, bool>> filtering, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Where(filtering)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdWithIncludesAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Include(e => e.LoyaltyAccounts)
            .ThenInclude(e => e.PointTransactions)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}