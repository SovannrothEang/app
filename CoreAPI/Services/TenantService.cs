using System.Formats.Asn1;
using System.Linq.Expressions;
using AutoMapper;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class TenantService(
    IUnitOfWork unitOfWork,
    IUserService userService,
    ICurrentUserProvider currentUserProvider,
    IMapper mapper,
    ILogger<TenantService> logger) : ITenantService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IRepository<Tenant> _repository = unitOfWork.GetRepository<Tenant>();
    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;
    private readonly IAccountTypeRepository _accountTypeRepository = unitOfWork.AccountTypeRepository;
    private readonly ITransactionTypeRepository _transactionTypeRepository = unitOfWork.TransactionTypeRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<TenantService> _logger = logger;

    public async Task<IEnumerable<TenantDto>> GetAllAsync(Expression<Func<Tenant, bool>>? filtering = null, CancellationToken cancellationToken = default)
    {
        var tenants = await _tenantRepository.GetAllAsync(filtering, cancellationToken);
        return tenants.Select(e => _mapper.Map<TenantDto>(e)).ToList();
    }

    public async Task<TenantDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct);
        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantOnboardResponseDto> CreateAsync(TenantOnBoardingDto dto, CancellationToken ct = default)
    {
        // var exist = await _tenantRepository.IsExistByNameAsync(dto.Name, ct);
        // if (exist)
        //     throw new BadHttpRequestException($"Tenant with name: {dto.Name} is already exist!");
        
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var tenant = _mapper.Map<Tenant>(dto.Tenant);
            await _repository.CreateAsync(tenant, ct);
            var tenantDto = _mapper.Map<TenantDto>(tenant);
            
            // Default Transaction types
            IEnumerable<TransactionType> types =
            [
                new("earn_id", "earn", "Earn", "Points earned from activities", 1, false, tenant.Id),
                new("redeem_id", "redeem", "Redeem", "Points redeems for rewards", -1, false, tenant.Id),
                new("adjust_id", "adjust", "Adjust", "Manual points adjustment", 1, false, tenant.Id),
            ];
            await _transactionTypeRepository.CreateBatchAsync(types, ct);

            // Default Account type
            var accountType = new AccountType(Guid.NewGuid().ToString(), "Normal", tenant.Id, _currentUserProvider.UserId);
            await _accountTypeRepository.CreateAccountTypeAsync(accountType, ct);
            
            var (userId, token) = await _userService.CreateTenantUserAsync(tenantDto, dto.Owner, ct);

            await _unitOfWork.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            return new TenantOnboardResponseDto(tenantDto, token);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
    
    public async Task UpdateAsync(string id, TenantUpdateDto dto, CancellationToken ct = default)
    {
        var exist = await _tenantRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        _mapper.Map(dto, exist);
        _repository.Update(exist);
        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var exist = await _tenantRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        _repository.Remove(exist);
        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task ActivateAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct)
                     ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");

        tenant.Activate();
        await _unitOfWork.CompleteAsync(ct);
    }
    
    public async Task DeactivateAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        tenant.Deactivate();
        await _unitOfWork.CompleteAsync(ct);
    }
}