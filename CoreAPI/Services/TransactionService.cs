using CoreAPI.DTOs.Customers;
using CoreAPI.Exceptions;
using CoreAPI.Models;
using CoreAPI.Models.Enums;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class TransactionService(
    IUnitOfWork unitOfWork,
    ICurrentUserProvider currentUserProvider,
    ILogger<TransactionService> logger)
    : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository =
        unitOfWork.TransactionRepository;

    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;

    private readonly ICustomerRepository _customerRepository =
        unitOfWork.CustomerRepository;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<TransactionService> _logger = logger;

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(
        CancellationToken ct = default)
    {
        return await _transactionRepository.GetAllAsync(cancellationToken: ct);
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
        return await _transactionRepository.GetByCustomerIdAsync(customerId, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByTenantIdAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
    }

    public async Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null) throw new KeyNotFoundException($"No Tenant was found with id: {tenantId}.");

        if (tenant.Status != TenantStatus.Active) throw new BadHttpRequestException("Tenant is not active!");

        var customer = await _customerRepository.GetByIdAsync(customerId, childIncluded: true, cancellationToken)
                       ?? throw new KeyNotFoundException( $"No Customer was found with id: {customerId}.");

        return (customer, tenant);
    }

    public async Task<(decimal balance, Transaction transactionDetail)>
        EarnPointAsync(
            string customerId,
            string tenantId,
            CustomerEarnPointDto dto,
            CancellationToken cancellationToken = default)
    {
        var performBy = _currentUserProvider.UserId;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Tenant with id: {TenantId} awards point to Customer with id: {CustomerId}, perform by user {PerformBy}.",
                tenantId, customerId, performBy);
        
        var (customer, tenant) =
            await GetValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Transaction transactionDetail;
            decimal balance;

            var account = customer.Accounts.FirstOrDefault(e => e.TenantId == tenant.Id);
            if (account is not null)
            {
                (balance, transactionDetail) = account.EarnPoint(dto.Amount, dto.Reason, null, performBy);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation(
                        "Creating account for Tenant with id: {TenantId} for Customer with id: {CustomerId}!",
                        tenantId, customerId);
                
                var newAccount = customer.CreateLoyaltyAccount(tenant.Id);
                (balance, transactionDetail) = newAccount.EarnPoint(dto.Amount, dto.Reason, null, performBy);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(
                    "Transaction was created with id: {TransactionId}, with Reason {Reason}.",
                    transactionDetail.Id, dto.Reason);
            return (balance, transactionDetail);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(decimal balance, Transaction transactionDetail)>
        RedeemPointAsync(
            string customerId,
            string tenantId,
            CustomerRedeemPointDto dto,
            CancellationToken cancellationToken = default)
    {
        var performBy = _currentUserProvider.UserId;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Redeeming point for tenant with id: {TenantId} and customer with id: {CustomerId} by user {PerformBy}",
                tenantId, customerId, performBy);
        
        var (customer, tenant) = await GetValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
        IsCustomerLinkedWithTenant(customer, tenant);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var account = customer.Accounts.FirstOrDefault(e => e.TenantId == tenant.Id);
            if (account is null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        "Invalid attempt! No account was found for Tenant {TenantId} and Customer {CustomerId}.",
                        tenantId, customerId);
                throw new BadHttpRequestException($"No account was found for Tenant {tenantId} and Customer {customerId}.");
            }
            
            if (dto.Amount > account.Balance)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        "Invalid amount attempt. Amount {Amount} is too much for Balance {Balance}.",
                        dto.Amount, account.Balance);
                throw new InsufficientBalanceException(account.Balance, dto.Amount);
            }
            var (balance, transactionDetail) = account.Redemption(dto.Amount, dto.Reason, performBy);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(
                    "Transaction was created with id: {TransactionId}, with Reason {Reason}",
                    transactionDetail.Id, dto.Reason);
            return (balance, transactionDetail);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(decimal balance, Transaction transactionDetail)>
        AdjustPointAsync(
            string customerId,
            string tenantId,
            CustomerAdjustPointDto dto,
            CancellationToken cancellationToken = default)
    {
        var performBy = _currentUserProvider.UserId;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Adjusting point for tenant with id: {TenantId} and customer with id: {CustomerId} by user {PerformBy}",
                tenantId, customerId, performBy);
        
        var (customer, tenant) = await GetValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
        IsCustomerLinkedWithTenant(customer, tenant);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var account = customer.Accounts.FirstOrDefault(e => e.TenantId == tenant.Id);
            if (account is null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(
                        "Invalid attempt! No account was found for Tenant {TenantId} and Customer {CustomerId}.",
                        tenantId, customerId);
                throw new BadHttpRequestException($"No account was found for Tenant {tenantId} and Customer {customerId}.");
            }
            
            var (balance, transactionDetail) = account.Adjustment(dto.Amount, dto.Reason, null, performBy);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(
                    "Transaction was created with id: {TransactionId}, with Reason {Reason}",
                    transactionDetail.Id, dto.Reason);
            return (balance, transactionDetail);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static void IsCustomerLinkedWithTenant(Customer customer, Tenant tenant)
    {
        var exist = customer.Accounts.Any(acc => acc.TenantId == tenant.Id);
        if (!exist)
            throw new KeyNotFoundException( $"Customer does not have account with tenant, tenant id: {tenant.Id}.");
    }
}