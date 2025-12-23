using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.Exceptions;
using CoreAPI.Models;
using CoreAPI.Models.Enums;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class PointTransactionService(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IPointTransactionService
{
    private readonly IPointTransactionRepository _pointTransactionRepository = unitOfWork.PointTransactionRepository;
    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;
    private readonly ICustomerRepository _customerRepository = unitOfWork.CustomerRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<PointTransaction>> GetAllTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await _pointTransactionRepository.GetAllAsync(cancellationToken);
        return transactions;
    }

    public async Task<CustomerDto> GetValidCustomerAsync(string customerId, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken: ct)
                       ?? throw new KeyNotFoundException($"No customer was found with id: {customerId}.");
        return _mapper.Map<CustomerDto>(customer);
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
       
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken: cancellationToken)
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
        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        PointTransaction transactionDetail;
        int balance;
            
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

        await _unitOfWork.CommitAsync(cancellationToken);
        return (balance, transactionDetail);
    }

    public async Task<(decimal balance, PointTransaction transactionDetail)> RedeemPointAsync(string customerId, string tenantId, CustomerRedeemPointDto dto,
        CancellationToken cancellationToken = default)
    {
        var (customer, tenant) = await ValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
        CheckRelationshipCustomerAndTenant(customer, tenant);
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var account = customer.LoyaltyAccounts.First(e => e.TenantId == tenant.Id);
        if (dto.Amount > account.Balance)
            throw new InsufficientBalanceException(account.Balance, dto.Amount);
        var (balance, transactionDetail) = account.Redemption(dto.Amount, dto.Reason);

        await _unitOfWork.CommitAsync(cancellationToken);
        return (balance, transactionDetail);
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
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var account = customer.LoyaltyAccounts.First(e => e.TenantId == tenant.Id);
        var (balance, transactionDetail) = account.Adjustment(dto.Amount, dto.Reason, null);

        await _unitOfWork.CommitAsync(cancellationToken);
        return (balance, transactionDetail);
    }
}