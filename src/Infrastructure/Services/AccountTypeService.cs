using Application.DTOs.Accounts;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AccountTypeService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserProvider currentUserProvider,
    ILogger<AccountTypeService> logger) : IAccountTypeService
{
    private readonly IRepository<AccountType> _repository = unitOfWork.GetRepository<AccountType>();
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<AccountTypeService> _logger = logger;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<AccountTypeDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[AccountTypeService] GetAllAsync performed by User {UserId}",
                _currentUserProvider.UserId);
        var types = await _repository.ListAsync(cancellationToken: ct);
        return types.Select(_mapper.Map<AccountTypeDto>);
    }

    public async Task<AccountTypeDto> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[AccountTypeService] GetByIdAsync called with ID: {AccountTypeId} by User {UserId}",
                id, _currentUserProvider.UserId);
        var accountType = await _repository.FirstOrDefaultAsync(
            c => c.Id == id,
            cancellationToken: ct);
        if (accountType is null)
        {
            _logger.LogWarning("No account type was found with ID {AccountTypeId}, performed by {Performer}",
                id, _currentUserProvider.UserId);
            throw new KeyNotFoundException($"Account type with ID {id} not found.");
        }
        return _mapper.Map<AccountTypeDto>(accountType);
    }

    public async Task<AccountTypeDto> CreateAsync(AccountTypeCreateDto dto, CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[AccountTypeService] CreateAsync performed by User {UserId}",
                _currentUserProvider.UserId);
        var accountType = new AccountType
        {
            Name = dto.Name,
        };
        await _repository.CreateAsync(accountType, ct);
        return _mapper.Map<AccountTypeDto>(accountType);
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
    {
        return await _repository.ExistsAsync(
            predicate: t => t.Id == id,
            cancellationToken: ct);
    }
}
