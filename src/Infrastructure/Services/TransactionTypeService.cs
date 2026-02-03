using Application.DTOs.Transactions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TransactionTypeService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserProvider currentUserProvider,
    ILogger<TransactionTypeService> logger) : ITransactionTypeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<TransactionTypeService> _logger = logger;
    private readonly IRepository<TransactionType> _repository = unitOfWork.GetRepository<TransactionType>();

    public async Task<IEnumerable<TransactionTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var types = await _repository.ListAsync(cancellationToken: cancellationToken);
        return types.Select(_mapper.Map<TransactionTypeDto>);
    }

    public async Task<IEnumerable<OperationDto>> GetAllOperationsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.ListAsync(
            filter: e => e.IsActive == true,
            select: e => new OperationDto(
                e.Slug,
                e.Name,
                e.Description,
                e.Url),
            cancellationToken: cancellationToken);
    }

    public async Task<TransactionTypeDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var type = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id && e.IsActive == true,
            cancellationToken: cancellationToken);
        if (type is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No transaction type was found with id: {TransactionTypeId}.", id);
            throw new KeyNotFoundException($"No transaction type was found with id: {id}.");
        }
        return _mapper.Map<TransactionTypeDto>(type);
    }

    public async Task<TransactionTypeDto> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var type = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Name == name && e.IsActive == true,
            cancellationToken: cancellationToken);
        if (type is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No transaction type was found with name: {TransactionTypeName}.", name);
            throw new KeyNotFoundException($"No transaction type was found with name: {name}.");
        }
        return _mapper.Map<TransactionTypeDto>(type);
    }

    public async Task<TransactionTypeDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var type = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Slug == slug && e.IsActive == true,
            cancellationToken: cancellationToken);
        if (type is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No transaction type was found with slug: {TransactionTypeSlug}.", slug);
            throw new KeyNotFoundException($"No transaction type was found with slug: {slug}.");
        }
        return _mapper.Map<TransactionTypeDto>(type);
    }

    public async Task<bool> IsExistAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(
            predicate: e => e.Id == id && e.IsActive == true,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> IsTypeExistAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(
            predicate: e => e.Name == name && e.IsActive == true,
            cancellationToken: cancellationToken);
    }

    public async Task<TransactionTypeDto> CreateAsync(
        TransactionTypeCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var type = _mapper.Map<TransactionType>(dto);
        await _repository.CreateAsync(type, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return _mapper.Map<TransactionTypeDto>(type);
    }

    public async Task<int> CreateBatchAsync(IEnumerable<TransactionTypeCreateDto> dtos,
        CancellationToken cancellationToken = default)
    {
        if (!dtos.Any())
            throw new InvalidOperationException("No type of transaction was found!");

        var types = dtos.Select(_mapper.Map<TransactionType>);
        await _repository.CreateBatchAsync(types, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return types.Count();
    }

    public async Task DeactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Deactivating transaction type with id: {TransactionTypeId}, perform by User {Performer}",
                id, _currentUserProvider.UserId);
        var type = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id && e.IsActive == true,
            trackChanges: true,
            cancellationToken: cancellationToken);
        if (type is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No transaction type was found with id: {TransactionTypeId}.", id);
            throw new KeyNotFoundException($"No transaction type was found with id: {id}.");
        }
        type.Deactivate();
        await _unitOfWork.CompleteAsync(cancellationToken);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Transaction type with id: {TransactionTypeId} has been deactivated.", id);
    }

    public async Task ActivateAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Activating transaction type with id: {TransactionTypeId}, perform by User {Performer}",
                id, _currentUserProvider.UserId);
        var type = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id && e.IsActive == true,
            trackChanges: true,
            cancellationToken: cancellationToken);
        if (type is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No transaction type was found with id: {TransactionTypeId}.", id);
            throw new KeyNotFoundException($"No transaction type was found with id: {id}.");
        }
        type.Deactivate();
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
