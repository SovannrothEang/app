using AutoMapper;
using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Customers;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class CustomerService(
    IUnitOfWork unitOfWork,
    ICustomerRepository customerRepository,
    IAccountRepository accountRepository,
    ICurrentUserProvider currentUserProvider,
    IUserService userService,
    IMapper mapper) : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(bool childIncluded = false, CancellationToken ct = default)
    {
        var customers =  await _customerRepository.GetAllAsync(childIncluded, cancellationToken: ct);
        return customers.Select(e => _mapper.Map<CustomerDto>(e));
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersPerTenantAsync(
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customers =  await _customerRepository.GetAllCustomersPerTenantAsync(childIncluded, cancellationToken: ct);
        return customers.Select(e => _mapper.Map<Customer,CustomerDto>(e));
    }

    public async Task<CustomerDto?> GetByIdAsync(string id, bool childIncluded = false, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, childIncluded, cancellationToken: ct);
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto?> GetByIdInTenantScopeAsync(
        string id,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdInTenantScopeAsync(id, childIncluded, cancellationToken: ct);
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<(decimal balance, List<Transaction> list)> GetCustomerBalanceByIdAsync(
        string customerId,
        string tenantId,
        CustomerGetBalanceOptionsDto? options,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByTenantAndCustomerAsync(
                          tenantId, customerId, childIncluded: true, cancellationToken)
                      ?? throw new KeyNotFoundException($"No Account was found for customer with id: {customerId}.");

        List<Transaction> lastActivities = [];
        

        if (options is not null)
        {
            // TODO: check the existence of the Transaction (maybe use name instead of ID)
            // if (options!.TransactionType is not null)
            // {
            //     lastActivities = account.Transactions.Where(t 
            //         => string.Equals(t.TransactionTypeId, options.TransactionType, StringComparison.InvariantCultureIgnoreCase)).ToList();
            // }
            if (options.StartDate is not null)
                lastActivities = [..account.Transactions.Where(p => p.OccurredAt >= options.StartDate.Value).ToList()];

            if (options.EndDate is not null)
                lastActivities = lastActivities.Where(act => act.OccurredAt <= options.EndDate.Value).ToList();
        }

        // Defect as always showing even query param is not null
        if (lastActivities.Count <= 0)
            lastActivities.Add(account.Transactions.Last());
        // Do a pagination

        return (account.Balance, lastActivities);
    }

    public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var registerDto = new RegisterDto(dto.UserName, dto.Email, dto.FirstName, dto.LastName, dto.Password, dto.Password);
            var user = await _userService.CreateUserAsync(registerDto, cancellationToken);
            var customer = new Customer(Guid.NewGuid().ToString(), user.Id, _currentUserProvider.UserId);

            await _customerRepository.CreateAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"Customer with id: {id} not found.");
        
        await _customerRepository.Update(customer);
        await _customerRepository.SaveChangeAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"Customer with id: {id} not found.");
        
        await _customerRepository.Remove(customer);
        await _customerRepository.SaveChangeAsync(ct);
    }
}