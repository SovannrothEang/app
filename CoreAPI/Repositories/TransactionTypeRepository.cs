using CoreAPI.Data;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Repositories;

public class TransactionTypeRepository(AppDbContext context) : ITransactionTypeRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<TransactionType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OperationDto>> GetAllOperationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .Where(e => e.IsActive == true)
            .Select(e => new OperationDto(
                e.Slug,
                e.Name,
                e.Description,
                e.Url))
            .ToListAsync(cancellationToken);
    }

    public async Task<TransactionType?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .Where(e => e.Id == id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TransactionType?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .Where(e => e.Name == name)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TransactionType?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .Where(e => e.Slug == slug)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsExistAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> IsTypeExistAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .AnyAsync(e => e.Name == name, cancellationToken);
    }

    public async Task CreateAsync(
        TransactionType type,
        CancellationToken cancellationToken = default)
    {
        await _context.TransactionTypes.AddAsync(type, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CreateBatchAsync(
        IEnumerable<TransactionType> types,
        CancellationToken cancellationToken = default)
    {
        await _context.AddRangeAsync(types, cancellationToken);
        var result = await _context.SaveChangesAsync(cancellationToken);
        return result;
    }
}
