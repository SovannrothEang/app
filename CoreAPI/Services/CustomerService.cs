using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.Models;
using CoreAPI.Repositories;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class CustomerService(
    IUnitOfWork unitOfWork,
    ICustomerRepository customerRepository,
    ITenantRepository tenantRepository,
    IAccountRepository accountRepository,
    IMapper mapper) : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(bool childIncluded = false, CancellationToken ct = default)
    {
        var customers =  await _customerRepository.GetAllAsync(childIncluded, cancellationToken: ct);
        return customers.Select(e => _mapper.Map<CustomerDto>(e));
    }

    public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken = default)
    {
        var customer = _mapper.Map<Customer>(dto);
        
        await _customerRepository.CreateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangeAsync(cancellationToken);
        return _mapper.Map<CustomerDto>(customer);
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

    public async Task<CustomerDto?> GetByIdAsync(string id, bool childIncluded = false, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, childIncluded, cancellationToken: ct);
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
        
        if (options!.TransactionType is not null)
        {
            lastActivities = account.PointTransactions.Where(t 
                => string.Equals(t.Type.ToString(), options.TransactionType, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (options.StartDate is not null)
            lastActivities = [..account.PointTransactions.Where(p => p.OccurredAt >= options.StartDate.Value).ToList()];
       
        if (options.EndDate is not null)
            lastActivities = lastActivities.Where(act => act.OccurredAt <= options.EndDate.Value).ToList();

        // Defect as always showing even query param is not null
        if (lastActivities.Count <= 0)
            lastActivities.Add(account.PointTransactions.Last());
        // Do a pagination

        return (account.Balance, lastActivities);
    }
}