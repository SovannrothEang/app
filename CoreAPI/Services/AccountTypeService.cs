using AutoMapper;
using CoreAPI.DTOs.Accounts;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class AccountTypeService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserProvider currentUserProvider) : IAccountTypeService
{
    private readonly IRepository<AccountType> _repository = unitOfWork.GetRepository<AccountType>();
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<AccountTypeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var types = await _repository.ListAsync(cancellationToken: ct);
        return types.Select(_mapper.Map<AccountTypeDto>);
    }

    public async Task<AccountTypeDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var accountType = await _repository.FirstOrDefaultAsync(
            c => c.Id == id,
            cancellationToken: ct);
        return _mapper.Map<AccountTypeDto>(accountType);
    }

    public async Task<AccountTypeDto> CreateAsync(AccountTypeCreateDto dto, CancellationToken ct = default)
    {
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
