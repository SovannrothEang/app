using CoreAPI.DTOs.Customers;
using CoreAPI.Exceptions;
using CoreAPI.Models;
using CoreAPI.Models.Enums;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class TransactionService(
    IUnitOfWork unitOfWork)
    : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository =
        unitOfWork.TransactionRepository;

    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;

    private readonly ICustomerRepository _customerRepository =
        unitOfWork.CustomerRepository;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(
        CancellationToken ct = default)
    {
        var transactions =
            await _transactionRepository.GetAllAsync(cancellationToken: ct);
        return transactions;
    }

    public async Task<IEnumerable<Transaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetAllByTenantAndCustomerAsync(
            tenantId,
            customerId,
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetAllByCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetAllByCustomerGlobalAsync(
            customerId,
            cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetByIdAsync(customerId, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByCustomerIdForTenantAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetByCustomerIdAsync(customerId,
            cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByTenantIdAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetByTenantIdAsync(tenantId,
            cancellationToken);
    }

    public async Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            throw new KeyNotFoundException($"No Tenant was found with id: {tenantId}.");

        if (tenant.Status != TenantStatus.Active)
            throw new BadHttpRequestException("Tenant is not active!");

        var customer = await _customerRepository.GetByIdAsync(customerId,
                           childIncluded: true, cancellationToken)
                       ?? throw new KeyNotFoundException(
                           $"No Customer was found with id: {customerId}.");

        return (customer, tenant);
    }

    public async Task<(decimal balance, Transaction transactionDetail)>
        EarnPointAsync(
            string customerId,
            string tenantId,
            CustomerEarnPointDto dto,
            CancellationToken cancellationToken = default)
    {
        var (customer, tenant) =
            await GetValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        Transaction transactionDetail;
        int balance;

        var account =
            customer.LoyaltyAccounts.FirstOrDefault(e => e.TenantId == tenant.Id);
        if (account is not null)
        {
            (balance, transactionDetail) =
                account.EarnPoint(dto.Amount, dto.Reason, null);
        }
        else
        {
            var newAccount = customer.CreateLoyaltyAccount(tenant.Id);
            (balance, transactionDetail) =
                newAccount.EarnPoint(dto.Amount, dto.Reason, null);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return (balance, transactionDetail);
    }

    public async Task<(decimal balance, Transaction transactionDetail)>
        RedeemPointAsync(
            string customerId,
            string tenantId,
            CustomerRedeemPointDto dto,
            CancellationToken cancellationToken = default)
    {
        var (customer, tenant) =
            await GetValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
        IsCustomerLinkedWithTenant(customer, tenant);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var account = customer.LoyaltyAccounts.First(e => e.TenantId == tenant.Id);
        if (dto.Amount > account.Balance)
            throw new InsufficientBalanceException(account.Balance, dto.Amount);
        var (balance, transactionDetail) = account.Redemption(dto.Amount, dto.Reason);

        await _unitOfWork.CommitAsync(cancellationToken);
        return (balance, transactionDetail);
    }

    public async Task<(decimal balance, Transaction transactionDetail)>
        AdjustPointAsync(
            string customerId,
            string tenantId,
            string? performBy,
            CustomerAdjustPointDto dto,
            CancellationToken cancellationToken = default)
    {
        var (customer, tenant) =
            await GetValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
        IsCustomerLinkedWithTenant(customer, tenant);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var account = customer.LoyaltyAccounts.First(e => e.TenantId == tenant.Id);
        var (balance, transactionDetail) =
            account.Adjustment(dto.Amount, dto.Reason, null);

        await _unitOfWork.CommitAsync(cancellationToken);
        return (balance, transactionDetail);
    }

    private static void IsCustomerLinkedWithTenant(Customer customer, Tenant tenant)
    {
        var exist = customer.LoyaltyAccounts.Any(acc => acc.TenantId == tenant.Id);
        if (!exist)
            throw new KeyNotFoundException(
                $"Customer does not has account with tenant, tenant id: {tenant.Id}.");
    }
}