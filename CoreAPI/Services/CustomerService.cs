using System.Linq.Expressions;
using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Services;

public class CustomerService(
    IUnitOfWork unitOfWork,
    IUserService userService,
    ITransactionService transactionService,
    IAccountService accountService,
    ICurrentUserProvider currentUserProvider,
    IMapper mapper) : ICustomerService
{
    #region Private Fields
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IRepository<Customer> _repository = unitOfWork.GetRepository<Customer>();
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IUserService _userService = userService;
    private readonly IAccountService _accountService = accountService;
    private readonly ITransactionService _transactionService = transactionService;
    #endregion

    public async Task<PagedResult<CustomerDto>> GetPaginatedResultsAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;

        var (customers, totalCount) = await _repository.GetPagedResultAsync(
            option,
            ignoreQueryFilters: true,
            filter: BuildFilter(option),
            includes: BuildIncludes(childIncluded, lastTransactionOnly: true),
            orderBy: BuildOrderBy(option),
            cancellationToken: ct);
        return new PagedResult<CustomerDto>
        {
            Items = [.. customers.Select(_mapper.Map<CustomerDto>)],
            PageNumber = option.Page.Value,
            PageSize = option.PageSize.Value,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<CustomerDto>> GetPaginatedResultsForTenantAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;

        var (customers, totalCount) = await _repository.GetPagedResultAsync(
            option,
            ignoreQueryFilters: true,
            filter: q => q.Accounts.Any(a => a.TenantId == _currentUserProvider.TenantId), // Only customers who have accounts with the current tenant
            includes: BuildIncludes(childIncluded, lastTransactionOnly: true),
            cancellationToken: ct);
        return new PagedResult<CustomerDto>
        {
            Items = [.. customers.Select(_mapper.Map<CustomerDto>)],
            PageNumber = option.Page.Value,
            PageSize = option.PageSize.Value,
            TotalCount = totalCount
        };
    }

    public async Task<CustomerDto?> GetByIdForGlobalAsync(
        string customerId,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var result = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == customerId,
            ignoreQueryFilters: true,
            includes: BuildIncludes(childIncluded, lastTransactionOnly: true),
            cancellationToken: ct);
        return _mapper.Map<CustomerDto>(result);
    }

    public async Task<CustomerDto> GetByIdForTenantAsync(
        string id,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        if (_currentUserProvider.TenantId is null)
            throw new UnauthorizedAccessException("TenantId is required for this operation.");
        
        var customer = await _repository.FirstOrDefaultAsync(
            predicate: e =>
                e.Id == id &&
                e.Accounts.Any(a => a.TenantId == _currentUserProvider.TenantId),
            ignoreQueryFilters: true,
            includes: BuildIncludes(childIncluded, lastTransactionOnly: true),
            cancellationToken: ct)
            ?? throw new KeyNotFoundException($"No Customer was found with id: {id}.");
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<(decimal balance, PagedResult<TransactionDto> result)>
        GetCustomerBalanceByIdAsync(
            string customerId,
            string tenantId,
            PaginationOption pageOption,
            bool childIncluded = false,
            CancellationToken ct = default)
    {
        // Init value if null
        pageOption.Page ??= 1;
        pageOption.PageSize ??= 10;

        var totalBalance = await _accountService.GetTotalBalanceByCustomerIdAsync(customerId, ct);
        var result = await _transactionService.GetAllByCustomerIdForTenantAsync(customerId, pageOption, childIncluded, ct);
        return (totalBalance, result);
    }

    public async Task<CustomerDto> CreateAsync(
        CustomerCreateDto dto,
        CancellationToken ct = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var registerDto = new RegisterDto(
                dto.UserName, dto.Email, dto.FirstName,
                dto.LastName, dto.Password, dto.ConfirmPassword);
            var user = await _userService.CreateUserAsync(registerDto, ct);

            var customer = new Customer(
                dto.Id ?? Guid.NewGuid().ToString(),
                user.Id, _currentUserProvider.UserId);
            await _repository.CreateAsync(customer, ct);

            await _unitOfWork.CompleteAsync(ct);
            await transaction.CommitAsync(ct);

            return _mapper.Map<CustomerDto>(customer);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task UpdateAsync(
        string id,
        CustomerUpdateDto dto,
        CancellationToken ct = default)
    {
        var customer = await _repository.FirstOrDefaultAsync(
            predicate: c => c.Id == id,
            trackChanges: true,
            ignoreQueryFilters: true,
            cancellationToken: ct)
           ?? throw new KeyNotFoundException($"No Customer was found with id: {id}.");

        _mapper.Map(dto, customer);
        _repository.Update(customer);
        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var customer = await _repository.FirstOrDefaultAsync(
            predicate: c => c.Id == id,
            trackChanges: true,
            ignoreQueryFilters: true,
            cancellationToken: ct)
           ?? throw new KeyNotFoundException($"No Customer was found with id: {id}.");
        // TODO: what should we do after customer is deleted? it's about account.

        customer.Deleted();
        await _unitOfWork.CompleteAsync(ct);
    }

    #region Helper Methods
    
    /// <summary>
    /// Builds filter expression for customer queries.
    /// Supports: id, username, email.
    /// </summary>
    private static Expression<Func<Customer, bool>>? BuildFilter(PaginationOption option)
    {
        if (string.IsNullOrEmpty(option.FilterBy) || string.IsNullOrEmpty(option.FilterValue))
            return null;

        return option.FilterBy.ToLower() switch
        {
            "id" => e => e.Id == option.FilterValue,
            "username" => e => e.User!.UserName == option.FilterValue,
            "email" => e => e.User!.Email == option.FilterValue,
            _ => null
        };
    }

    /// <summary>
    /// Builds includes for customer queries.
    /// Always includes User. Optionally includes Accounts with Transactions and AccountType.
    /// </summary>
    private static Func<IQueryable<Customer>, IQueryable<Customer>> BuildIncludes(
        bool childIncluded,
        bool lastTransactionOnly = false)
    {
        return queryable =>
        {
            queryable = queryable.Include(c => c.User);
            if (childIncluded)
            {
                queryable = queryable
                    .Include(c => c.Accounts)
                        .ThenInclude(acc => acc.AccountType)
                    .Include(c => c.Accounts)
                        .ThenInclude(acc => lastTransactionOnly
                            ? acc.Transactions.OrderByDescending(x => x.CreatedAt).Take(1)
                            : acc.Transactions.OrderByDescending(x => x.CreatedAt));
            }
            return queryable;
        };
    }

    /// <summary>
    /// Build order by for customer queries.
    /// Supports: name, email, tenant, createdAt. Defaults to createdAt ascending.
    /// </summary>
    private static Func<IQueryable<Customer>, IOrderedQueryable<Customer>> BuildOrderBy(PaginationOption option)
    {
        var sortBy = option.SortBy?.ToLower() ?? "createdat";
        var sortDirection = option.SortDirection?.ToLower() ?? "asc";
        return (sortBy, sortDirection) switch
        {
            ("name", "asc") => q => q.OrderBy(c => c.User!.FirstName).ThenBy(c => c.User!.LastName),
            ("name", "desc") => q => q.OrderByDescending(c => c.User!.FirstName).ThenByDescending(c => c.User!.LastName),
            ("email", "asc") => q => q.OrderBy(c => c.User!.Email).ThenBy(u => u.CreatedAt),
            ("email", "desc") => q => q.OrderByDescending(c => c.User!.Email).ThenBy(u => u.CreatedAt),
            ("tenant", "asc") => q => q.OrderBy(c => c.Tenant!.Name).ThenBy(t => t.Id),
            ("tenant", "desc") => q => q.OrderByDescending(c => c.Tenant!.Name).ThenBy(t => t.Id),
            ("createdat", "asc") => q => q.OrderBy(c => c.CreatedAt),
            ("createdat", "desc") => q => q.OrderByDescending(c => c.CreatedAt),
            _ => q => q.OrderBy(c => c.CreatedAt).ThenBy(c => c.User!.FirstName).ThenBy(c => c.User!.LastName)
        };
    }

    #endregion
}
