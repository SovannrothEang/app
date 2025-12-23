using System.Linq.Expressions;
using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.Exceptions;
using CoreAPI.Models;
using CoreAPI.Models.Enums;
using CoreAPI.Repositories;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class CustomerService(
    IUnitOfWork unitOfWork,
    ICustomerRepository customerRepository,
    ITenantRepository tenantRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    IMapper mapper) : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly ILoyaltyAccountRepository _loyaltyAccountRepository = loyaltyAccountRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(
        Expression<Func<Customer, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllWithFiltering(predicate, cancellationToken);
        return customers.Select(customer => _mapper.Map<CustomerDto>(customer));
    }

    public Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<CustomerDto>> GetAllWithIncludesAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken = default)
    {
        var customer = _mapper.Map<Customer>(dto);
        
        await _customerRepository.CreateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangeAsync(cancellationToken);
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto?> GetByIdAsync(string id, bool? childIncluded = false, CancellationToken ct = default)
    {
        var customer = childIncluded is false or null
            ? await _customerRepository.GetByIdAsync(id, ct)
            : await _customerRepository.GetByIdWithIncludesAsync(id, ct);
        
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> GetValidCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
                       ?? throw new KeyNotFoundException($"No customer was found with id: {customerId}.");
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<(decimal balance, List<PointTransaction> list)> GetCustomerBalanceByIdAsync(
        string customerId,
        string tenantId,
        CustomerGetBalanceOptionsDto? options,
        CancellationToken cancellationToken = default)
    {
        var account = await _loyaltyAccountRepository.GetByTenantAndCustomerAsync(
                          tenantId, customerId, cancellationToken)
                      ?? throw new KeyNotFoundException($"No Account was found for customer with id: {customerId}.");

        List<PointTransaction> lastActivities = [];
        
        if (options!.TransactionType is not null)
        {
            lastActivities = account.PointTransactions.Where(t 
                => string.Equals(t.Type.ToString(), options.TransactionType, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (options.StartDate is not null)
            lastActivities = [..account.PointTransactions.Where(p => p.OccurredOn >= options.StartDate.Value).ToList()];
       
        if (options.EndDate is not null)
            lastActivities = lastActivities.Where(act => act.OccurredOn <= options.EndDate.Value).ToList();

        // Defect as always showing even query param is not null
        if (lastActivities.Count <= 0)
            lastActivities.Add(account.PointTransactions.Last());
        // Do a pagination

        return (account.Balance, lastActivities);
    }
    
    public async Task<(Customer customer, Tenant tenant)> ValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null) 
            throw new KeyNotFoundException($"No Tenant was found with id: {tenantId}.");
       
        if (tenant.Status != TenantStatus.Active)
            throw new BadHttpRequestException("Tenant is not active!");
       
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
                       ?? throw new KeyNotFoundException($"No Customer was found with id: {customerId}.");

        return (customer, tenant);
    }

    private static void CheckRelationshipCustomerAndTenant(Customer customer, Tenant tenant)
    {
        var exist = customer.LoyaltyAccounts.Any(acc => acc.TenantId == tenant.Id);
        if (!exist)
            throw new KeyNotFoundException($"Customer does not has account with tenant, tenant id: {tenant.Id}.");
    }

    public async Task<(decimal balance, PointTransaction transactionDetail)>
        EarnPointAsync
        (string customerId, string tenantId, CustomerEarnPointDto dto, CancellationToken cancellationToken = default)
    {
        var (customer, tenant) = await ValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
        
        await using var tran = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            PointTransaction transactionDetail;
            decimal balance;
            
            var account = customer.LoyaltyAccounts.FirstOrDefault(e => e.TenantId == tenant.Id);
            if (account is not null)
            {
                (balance, transactionDetail) = account.EarnPoint(dto.Amount, dto.Reason, null);
            }
            else
            {
                var newAccount = customer.CreateLoyaltyAccount(tenant.Id);
                (balance, transactionDetail) = newAccount.EarnPoint(dto.Amount, dto.Reason, null);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            return (balance, transactionDetail);
        }
        catch
        {
            await tran.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(decimal balance, PointTransaction transactionDetail)> RedeemPointAsync(string customerId, string tenantId, CustomerRedeemPointDto dto,
        CancellationToken cancellationToken = default)
    {
        var (customer, tenant) = await ValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
        CheckRelationshipCustomerAndTenant(customer, tenant);
        await using var tran = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var account = customer.LoyaltyAccounts.First(e => e.TenantId == tenant.Id);
            if (dto.Amount > account.Balance)
                throw new InsufficientBalanceException(account.Balance, dto.Amount);
            var (balance, transactionDetail) = account.Redemption(dto.Amount, dto.Reason);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            return (balance, transactionDetail);
        }
        catch
        {
            await tran.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(decimal balance, PointTransaction transactionDetail)> AdjustPointAsync(
        string customerId,
        string tenantId,
        string? performBy,
        CustomerAdjustPointDto dto,
        CancellationToken cancellationToken = default)
    {
        // Checking relationship between customer and tenant
        // checking if tenant is deactivated
        var (customer, tenant) = await ValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
        CheckRelationshipCustomerAndTenant(customer, tenant);
        await using var tran = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var account = customer.LoyaltyAccounts.First(e => e.TenantId == tenant.Id);
            var (balance, transactionDetail) = account.Adjustment(dto.Amount, dto.Reason, null);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            return (balance, transactionDetail);
        }
        catch
        {
            await tran.RollbackAsync(cancellationToken);
            throw;
        }
    }
}