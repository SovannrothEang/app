using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class CustomerService(
    IUnitOfWork unitOfWork,
    IUserService userService,
    ICurrentUserProvider currentUserProvider,
    IMapper mapper) : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICustomerRepository _customerRepository = unitOfWork.CustomerRepository;
    private readonly IAccountRepository _accountRepository = unitOfWork.AccountRepository;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(bool childIncluded = false, CancellationToken ct = default)
    {
        var customers =  await _customerRepository.GetAllAsync(childIncluded, cancellationToken: ct);
        return customers.Select(e => _mapper.Map<CustomerDto>(e));
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersForTenantAsync(
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customers =  await _customerRepository.GetAllForTenantAsync(childIncluded, cancellationToken: ct);
        return customers.Select(e => _mapper.Map<Customer,CustomerDto>(e));
    }

    public async Task<CustomerDto> GetByIdForCustomerAsync(string id, bool childIncluded = false, CancellationToken ct = default)
    {
        if (_currentUserProvider.CustomerId != id)
            throw new BadHttpRequestException("Why are u here?");
        
        var customer = await _customerRepository.GetByIdForCustomerAsync(id, childIncluded, cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"Customer with id: {id} not found.");
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> GetByIdForTenantAsync(
        string id,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdForTenantAsync(id, childIncluded, cancellationToken: ct)
                       ?? throw  new KeyNotFoundException($"Customer with id: {id} not found.");
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<(decimal balance, PagedResult<TransactionDto> list)> GetCustomerBalanceByIdAsync(
        string customerId,
        string tenantId,
        CustomerGetBalanceOptionsDto? option,
        PaginationOption pageOption,
        CancellationToken cancellationToken = default)
    {
        var (account, transactions, totalCount) = await _accountRepository.GetByTenantAndCustomerPaginationAsync(
            tenantId,
            customerId,
            option,
            pageOption,
            childIncluded: true,
            cancellationToken);
        
        if (account is null)
            throw new KeyNotFoundException($"Customer with id: {customerId} not found.");


        // TODO: move this, or modified the options in the PaginationOption for sorting base on this
        // List<Transaction> lastActivities = [];
        // if (option is not null)
        // {
        //     if (option.TransactionType is not null)
        //     {
        //         lastActivities = transactions.Where(t 
        //             => string.Equals(t.TransactionType!.Slug, option.TransactionType, StringComparison.InvariantCultureIgnoreCase)).ToList();
        //     }
        //     if (option.StartDate is not null)
        //         lastActivities = [..account.Transactions.Where(p => p.OccurredAt >= option.StartDate.Value).ToList()];
        //
        //     if (option.EndDate is not null)
        //         lastActivities = lastActivities.Where(act => act.OccurredAt <= option.EndDate.Value).ToList();
        // }

        // Defect as always showing even query param is not null
        // if (lastActivities.Count <= 0)
        //     lastActivities.Add(account.Transactions.Last());
        
        var dtos = transactions.Select(a => _mapper.Map<TransactionDto>(a)).ToList();

        return (account.Balance, new PagedResult<TransactionDto>
        {
            Items = dtos,
            PageNumber = pageOption.Page,
            PageSize = pageOption.PageSize,
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

            await _customerRepository.CreateAsync(customer, cancellationToken);
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
        await _customerRepository.Update(customer);
        await _customerRepository.SaveChangeAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdForCustomerAsync(id, cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"Customer with id: {id} not found.");
        
        // TODO: what should we do after customer is deleted? it's about account.
        
        await _customerRepository.Remove(customer);
        await _customerRepository.SaveChangeAsync(ct);
    }
}