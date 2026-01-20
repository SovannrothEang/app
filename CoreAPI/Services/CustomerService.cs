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
    ICurrentUserProvider currentUserProvider,
    IMapper mapper) : ICustomerService
{
    #region Private Fields
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<Customer> _repository = unitOfWork.GetRepository<Customer>();
    private readonly IAccountRepository _accountRepository = unitOfWork.AccountRepository;
    private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;
    #endregion

    #region Private Methods
    private static IQueryable<Customer> GetCustomerIncludes(IQueryable<Customer> queryable, bool childIncluded)
    {
        queryable.Include(c => c.User);
        if (childIncluded)
            queryable = queryable.Include(c => c.Accounts)
                    .ThenInclude(acc => acc.Transactions)
                .Include(c => c.Accounts)
                    .ThenInclude(acc => acc.AccountType);
        return queryable;
    } 
    #endregion

    // TODO: add order by logic
    public async Task<PagedResult<CustomerDto>> GetPaginatedResultsAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;

        var customers = await _repository.GetPagedAsync<CustomerDto>(
            page: option.Page.Value,
            pageSize: option.PageSize.Value,
            ignoreQueryFilters: true,
            filter: option.FilterBy is null
                ? null
                : option.FilterBy.ToLower() switch
                {
                    "id" => e => e.Id == option.FilterValue,
                    "username" => e => e.User!.UserName == option.FilterValue,
                    "email" => e => e.User!.Email == option.FilterValue,
                    _ => null
                },
            includes: q => GetCustomerIncludes(q, childIncluded),
            cancellationToken: ct);
        return customers;
    }

    public async Task<PagedResult<CustomerDto>> GetPaginatedResultsForTenantAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;

        var customers = await _repository.GetPagedAsync<CustomerDto>(
            page: option.Page.Value,
            pageSize: option.PageSize.Value,
            ignoreQueryFilters: true,
            filter: q => q.Accounts.Any(a => a.TenantId == _currentUserProvider.TenantId), // Only customers who have accounts with the current tenant
            includes: q => GetCustomerIncludes(q, childIncluded),
            cancellationToken: ct);
        return customers;
    }

    public async Task<CustomerDto> GetByIdForCustomerAsync(
        string customerId,
        PaginationOption? option,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customer = await _repository.FirstOrDefaultAsync<CustomerDto>(
            predicate: e => e.Id == customerId,
            ignoreQueryFilters: true,
            includes: q => GetCustomerIncludes(q, childIncluded),
            cancellationToken: ct)
           ?? throw new KeyNotFoundException($"No Customer was found with id: {customerId}.");
        return customer;
    }

    public async Task<CustomerDto> GetByIdForTenantAsync(
        string id,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        if (_currentUserProvider.TenantId is null)
            throw new UnauthorizedAccessException("TenantId is required for this operation.");
        var customer = await _repository.FirstOrDefaultAsync<CustomerDto>(
            predicate: e =>
                e.Id == id &&
                e.Accounts.Any(a => a.TenantId == _currentUserProvider.TenantId),
            ignoreQueryFilters: true,
            includes: q => GetCustomerIncludes(q, childIncluded),
            cancellationToken: ct)
           ?? throw new KeyNotFoundException($"No Customer was found with id: {id}.");
        return customer;
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

        // I dont need total accounts right now
        // TODO: make a method for getting all balance
        var (accounts, _) = await _accountRepository.GetAllByCustomerIdForGlobalAsync(
            customerId, pageOption, null, false, ct);
        var (result, totalCount) = await _transactionRepository.GetAllByCustomerIdForGlobalAsync(
            customerId, pageOption, childIncluded, ct);

        if (accounts is null || !accounts.Any())
            throw new KeyNotFoundException($"Customer with id: {customerId} not found.");

        var dtos = result.Select(_mapper.Map<TransactionDto>).ToList();

        return (accounts.Sum(a => a.Balance), new PagedResult<TransactionDto>
        {
            Items = dtos,
            PageNumber = pageOption.Page.Value,
            PageSize = pageOption.PageSize.Value,
            TotalCount = totalCount
        });
    }

    public async Task<CustomerDto> CreateAsync(
        CustomerCreateDto dto,
        CancellationToken ct = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var registerDto = new RegisterDto(
                dto.UserName,
                dto.Email,
                dto.FirstName,
                dto.LastName,
                dto.Password,
                dto.Password);
            var user = await _userService.CreateUserAsync(registerDto, ct);

            var customer = new Customer(
                dto.Id ?? Guid.NewGuid().ToString(),
                user.Id,
                _currentUserProvider.UserId);
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
}