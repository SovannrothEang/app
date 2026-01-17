using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class AccountTypeRepository(AppDbContext context) : IAccountTypeRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<AccountType>> GetAllAsync(bool childIncluded, CancellationToken cancellationToken)
    {
        var queryable = _context.AccountTypes
            .AsNoTracking()
            .AsQueryable();
        
        if (childIncluded)
            queryable = queryable
                .Include(e => e.Accounts)
                .Include(e => e.Performer);
        
        return await queryable.ToListAsync(cancellationToken);
    }

    public async Task<AccountType?> GetByIdAsync(string id, bool childIncluded, CancellationToken cancellationToken)
    {
        var queryable = _context.AccountTypes
            .AsNoTracking()
            .AsQueryable();

        if (childIncluded)
            queryable = queryable
                .Include(e => e.Accounts)
                .Include(e => e.Performer);
        
        return await queryable.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.AccountTypes
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task CreateAccountTypeAsync(AccountType newAccountType, CancellationToken cancellationToken = default)
    {
        await _context.AccountTypes.AddAsync(newAccountType, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}