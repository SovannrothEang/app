using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class AccountTypeRepository(AppDbContext context) : IAccountTypeRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<AccountType>> GetAccountTypesAsync(CancellationToken cancellationToken)
    {
        return await _context.AccountTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<AccountType?> GetAccountTypeAsync(string id, CancellationToken cancellationToken)
    {
        return await _context.AccountTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
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