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
    private readonly ICustomerRepository _customerRepository = unitOfWork.CustomerRepository;
    private readonly IAccountRepository _accountRepository = unitOfWork.AccountRepository;
    private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;
    #endregion

    // TODO: add order by logic
    public async Task<IEnumerable<CustomerDto>> GetAllForGobalAsync(
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customers = await _repository.ListAsync(
            ignoreQueryFilters: true,
            includes: q =>
            {
                q.Include(c => c.User);
                if (childIncluded)
                    q = q
                        .Include(c => c.Accounts)
                            .ThenInclude(acc => acc.Transactions)
                        .Include(c => c.Accounts)
                            .ThenInclude(acc => acc.AccountType);
                return q;
            },
            filter: null, // Global usage
            cancellationToken: ct);
        return customers.Select(e => _mapper.Map<CustomerDto>(e));
    }
    public async Task<IEnumerable<CustomerDto>> GetAllForTenantAsync(
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customers = await _repository.ListAsync(
            ignoreQueryFilters: true,
            includes: q =>
            {
                q.Include(c => c.User);
                if (childIncluded)
                    q = q
                        .Include(c => c.Accounts)
                            .ThenInclude(acc => acc.Transactions)
                        .Include(c => c.Accounts)
                            .ThenInclude(acc => acc.AccountType);
                return q;
            },
            filter: q => q.Accounts.Any(a => a.TenantId == _currentUserProvider.TenantId), // Only customers who have accounts with the current tenant
            cancellationToken: ct);
        return customers.Select(e => _mapper.Map<CustomerDto>(e));
    }

    public async Task<CustomerDto> GetByIdForCustomerAsync(
        string customerId,
        PaginationOption? option,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customer = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == customerId,
            ignoreQueryFilters: true,
            includes: q =>
            {
                q.Include(c => c.User);
                if (childIncluded)
                    q = q
                        .Include(c => c.Accounts)
                            .ThenInclude(acc => acc.Transactions)
                        .Include(c => c.Accounts)
                            .ThenInclude(acc => acc.AccountType);
                return q;
            },
            cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"Customer with id: {customerId} not found.");
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> GetByIdForTenantAsync(
        string id,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customer = await _repository.FirstOrDefaultAsync(
            predicate: e =>
                e.Id == id &&
                e.Accounts.Any(a => a.TenantId == _currentUserProvider.TenantId),
            ignoreQueryFilters: true,
            includes: q =>
            {
                q.Include(c => c.User);
                if (childIncluded)
                    q = q
                        .Include(c => c.Accounts)
                            .ThenInclude(acc => acc.Transactions)
                        .Include(c => c.Accounts)
                            .ThenInclude(acc => acc.AccountType);
                return q;
            },
            cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"Customer with id: {id} not found.");
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<(decimal balance, PagedResult<TransactionDto> result)>
        GetCustomerBalanceByIdAsync(
            string customerId,
            string tenantId,
            PaginationOption pageOption,
            bool childIncluded = false,
            CancellationToken cancellationToken = default)
    {
        // Init value if null
        pageOption.Page ??= 1;
        pageOption.PageSize ??= 10;

        // I dont need total accounts right now
        // TODO: make a method for getting all balance
        var (accounts, _) = await _accountRepository.GetAllByCustomerIdForGlobalAsync(
            customerId, pageOption, null, false, cancellationToken);
        var (result, totalCount) = await _transactionRepository.GetAllByCustomerIdForGlobalAsync(
            customerId, pageOption, childIncluded, cancellationToken);

        if (accounts is null || !accounts.Any())
            throw new KeyNotFoundException($"Customer with id: {customerId} not found.");

        var dtos = result.Select(acc => _mapper.Map<TransactionDto>(acc)).ToList();

        return (accounts.Sum(acc => acc.Balance), new PagedResult<TransactionDto>
        {
            Items = dtos,
            PageNumber = pageOption.Page.Value,
            PageSize = pageOption.PageSize.Value,
            TotalCount = totalCount
        });
    }

    public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var registerDto = new RegisterDto(dto.UserName, dto.Email, dto.FirstName, dto.LastName, dto.Password, dto.Password);
            var user = await _userService.CreateUserAsync(registerDto, cancellationToken);
            var customer = new Customer(dto.Id ?? Guid.NewGuid().ToString(), user.Id, _currentUserProvider.UserId);

            await _repository.CreateAsync(customer, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return _mapper.Map<CustomerDto>(customer);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(string id, CustomerUpdateDto dto, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdForCustomerAsync(id, cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"Customer with id: {id} not found.");

        _mapper.Map(dto, customer);
        _repository.Update(customer);
        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdForCustomerAsync(id, cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"Customer with id: {id} not found.");

        // TODO: what should we do after customer is deleted? it's about account.

        customer.Deleted();
        await _unitOfWork.CompleteAsync(ct);
    }
}