using System.Linq.Expressions;
using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;
using CoreAPI.Models.Enums;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Services;

public class TenantService(
    IUnitOfWork unitOfWork,
    IUserService userService,
    ICurrentUserProvider currentUserProvider,
    IMapper mapper,
    ILogger<TenantService> logger) : ITenantService
{
    #region Private Fields
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;
    private readonly IRepository<AccountType> _accountTypeRepository = unitOfWork.GetRepository<AccountType>();
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IRepository<Tenant> _repository = unitOfWork.GetRepository<Tenant>();
    private readonly IRepository<TransactionType> _transactionTypeRepository = unitOfWork.GetRepository<TransactionType>();
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<TenantService> _logger = logger;
    #endregion

    public async Task<PagedResult<TenantDto>> GetPagedResultsAsync(PaginationOption option, CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;

        // TODO: add orderby logic
        var result = await _repository.GetPagedResultAsync<TenantDto>(
            option,
            ignoreQueryFilters: true,
            filter: BuildFilter(option),
            includes: BuildIncludes(),
            cancellationToken: ct
        );
        return result;
    }

    public async Task<TenantDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _repository.FirstOrDefaultAsync<TenantDto>(
            predicate: e => e.Id == id,
            cancellationToken: ct);
    }

    public async Task<TenantDto?> GetValidTenantByIdAsync(string id, bool trackChange = false, CancellationToken ct = default)
    {
        return await _repository.FirstOrDefaultAsync<TenantDto>(
            predicate: e => e.Id == id && e.Status == TenantStatus.Active,
            trackChanges: trackChange,
            cancellationToken: ct);
    }

    public async Task<TenantOnboardResponseDto> CreateAsync(TenantOnBoardingDto dto, CancellationToken ct = default)
    {
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
            await _accountTypeRepository.CreateAsync(accountType, ct);
            
            var (userId, token) = await _userService.CreateTenantUserAsync(tenantDto, dto.Owner, ct);

            await _unitOfWork.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            return new TenantOnboardResponseDto(tenantDto, userId, token);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
    
    public async Task UpdateAsync(string id, TenantUpdateDto dto, CancellationToken ct = default)
    {
        var exist = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            trackChanges: true,
            cancellationToken: ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        _mapper.Map(dto, exist);
        _repository.Update(exist);
        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            trackChanges: true,
            cancellationToken: ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        tenant.Deleted();
        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task ActivateAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            trackChanges: true,
            cancellationToken: ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");

        tenant.Activate();
        await _unitOfWork.CompleteAsync(ct);
    }
    
    public async Task DeactivateAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            trackChanges: true,
            cancellationToken: ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        tenant.Deactivate();
        await _unitOfWork.CompleteAsync(ct);
    }

    #region Private Methods
    
    /// <summary>
    /// Builds filter expression for tenant queries.
    /// Supports: id, name, performby.
    /// </summary>
    private static Expression<Func<Tenant, bool>>? BuildFilter(PaginationOption option)
    {
        if (string.IsNullOrEmpty(option.FilterBy) || string.IsNullOrEmpty(option.FilterValue))
            return null;

        return option.FilterBy.ToLower() switch
        {
            "id" => e => e.Id == option.FilterValue,
            "name" => e => e.Name == option.FilterValue,
            "performby" => e => e.PerformBy == option.FilterValue,
            _ => null
        };
    }

    /// <summary>
    /// Builds includes for tenant queries.
    /// Includes Accounts with AccountType.
    /// </summary>
    private static Func<IQueryable<Tenant>, IQueryable<Tenant>> BuildIncludes()
    {
        return queryable => queryable
            .Include(t => t.Accounts)
            .ThenInclude(a => a.AccountType);
    }
    
    #endregion
}
