using AutoMapper;
using AutoMapper.QueryableExtensions;
using CoreAPI.Data;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Services.Interfaces;

public interface ITransactionTypeService
{
    Task<IEnumerable<TransactionTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TransactionTypeDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OperationDto>> GetAllOperationsAsync(CancellationToken cancellationToken = default);
    Task<TransactionTypeDto?> GetByNameAsync(string name,  CancellationToken cancellationToken = default);
    Task<TransactionTypeDto?> GetBySlugAsync(string slug,  CancellationToken cancellationToken = default);
    Task<bool> IsExistAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> IsTypeExistAsync(string name, CancellationToken cancellationToken = default);
    Task<TransactionTypeDto> CreateAsync(TransactionTypeCreateDto dto, CancellationToken cancellationToken = default);
}

public class TransactionTypeService(AppDbContext context, IMapper mapper) : ITransactionTypeService
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TransactionTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .ProjectTo<TransactionTypeDto>(_mapper.ConfigurationProvider)
            .ToArrayAsync(cancellationToken);
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

    public async Task<TransactionTypeDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .Where(e => e.Id == id)
            .ProjectTo<TransactionTypeDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TransactionTypeDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .Where(e => e.Name == name)
            .ProjectTo<TransactionTypeDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TransactionTypeDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionTypes
            .AsNoTracking()
            .Where(e => e.Slug == slug)
            .ProjectTo<TransactionTypeDto>(_mapper.ConfigurationProvider)
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

    public async Task<TransactionTypeDto> CreateAsync(
        TransactionTypeCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var type = _mapper.Map<TransactionType>(dto);
        await _context.TransactionTypes.AddAsync(type, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TransactionTypeDto>(type);
    }
}