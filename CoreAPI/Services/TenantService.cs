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

        var (items, totalCount) = await _repository.GetPagedResultAsync(
            option,
            ignoreQueryFilters: true,
            filter: BuildFilter(option),
            includes: BuildIncludes(),
            orderBy: BuildOrderBy(option),
            cancellationToken: ct
        );
        return new PagedResult<TenantDto>()
        {
            Items = [.. items.Select(_mapper.Map<TenantDto>)],
            PageNumber = option.Page.Value,
            PageSize = option.PageSize.Value,
            TotalCount = totalCount
        };
    }

    public async Task<TenantDto> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            includes: q => q.Include(e => e.AccountTypes),
            cancellationToken: ct);
        if (tenant is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("[TenantService] GetByIdAsync: No tenant found with id: {TenantId}", id);
            throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        }
        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantDto> GetValidTenantByIdAsync(string id, bool trackChange = false, CancellationToken ct = default)
    {
        var tenant = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id && e.Status == TenantStatus.Active,
            trackChanges: trackChange,
            cancellationToken: ct);
        if (tenant is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("[TenantService] GetValidTenantByIdAsync: No active tenant found with id: {TenantId}", id);
            throw new KeyNotFoundException($"No active tenant was found with id: {id}.");
        }
        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantOnboardResponseDto> CreateAsync(TenantOnBoardingDto dto, CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Onboarding new tenant: {TenantName}, Owner: {OwnerEmail}",
                dto.Tenant.Name, dto.Owner.Email);
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var tenant = _mapper.Map<Tenant>(dto.Tenant);
            await _repository.CreateAsync(tenant, ct);
            var tenantDto = _mapper.Map<TenantDto>(tenant);
        
            // Default Transaction types
            IEnumerable<TransactionType> types =
            [
                new(Guid.NewGuid().ToString(), "earn", "Earn", "Points earned from activities", 1, false, tenant.Id),
                new(Guid.NewGuid().ToString(), "redeem", "Redeem", "Points redeems for rewards", -1, false, tenant.Id),
                new(Guid.NewGuid().ToString(), "adjust", "Adjust", "Manual points adjustment", 1, true, tenant.Id),
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
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[TenantService] Updating tenant with id: {TenantId} by {Performer}", id, _currentUserProvider.UserId);
        var exist = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            trackChanges: true,
            cancellationToken: ct);
        if (exist is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No tenant found with id: {TenantId}", id);
            throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        }
        
        _mapper.Map(dto, exist);
        _repository.Update(exist);
        await _unitOfWork.CompleteAsync(ct);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Tenant with id: {TenantId} updated successfully", id);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            trackChanges: true,
            cancellationToken: ct);
        if (tenant is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No tenant found with id: {TenantId}", id);
            throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        }
        tenant.Deleted();
        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task ActivateAsync(string id, CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Activating tenant with id: {TenantId} by {Performer}", id, _currentUserProvider.UserId);
        var tenant = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            trackChanges: true,
            cancellationToken: ct);
        if (tenant is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No tenant found with id: {TenantId}", id);
            throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        }

        tenant.Activate();
        await _unitOfWork.CompleteAsync(ct);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Tenant with id: {TenantId} activated successfully", id);
    }
    
    public async Task DeactivateAsync(string id, CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Deactivating tenant with id: {TenantId} by {Performer}", id, _currentUserProvider.UserId);
        var tenant = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == id,
            trackChanges: true,
            cancellationToken: ct);

        if (tenant is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No tenant found with id: {TenantId}", id);
            throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        }
        
        tenant.Deactivate();
        await _unitOfWork.CompleteAsync(ct);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Tenant with id: {TenantId} deactivated successfully", id);
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
    
    private static Func<IQueryable<Tenant>, IOrderedQueryable<Tenant>> BuildOrderBy(PaginationOption option)
    {
        var sortBy = string.IsNullOrEmpty(option.SortBy) ? "name" : option.SortBy.ToLower();
        var sortDirection = string.IsNullOrEmpty(option.SortDirection) ? "asc" : option.SortDirection.ToLower();
        
        return sortBy switch
        {
            "name" => sortDirection == "asc"
                ? queryable => queryable.OrderBy(t => t.Name).ThenBy(t => t.CreatedAt)
                : queryable => queryable.OrderByDescending(t => t.Name).ThenBy(t => t.CreatedAt),
            "createdat" => sortDirection == "asc"
                ? queryable => queryable.OrderBy(t => t.CreatedAt)
                : queryable => queryable.OrderByDescending(t => t.CreatedAt),
            _ => queryable => queryable.OrderBy(t => t.Name)
        };
    }
    #endregion
}