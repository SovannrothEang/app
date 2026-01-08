using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;
using CoreAPI.Models.Enums;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using CoreAPI.Validators.Customers;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Services;

public class TransactionService(
    IUnitOfWork unitOfWork,
    ICurrentUserProvider currentUserProvider,
    ILogger<TransactionService> logger,
    IMapper mapper,
    ITransactionTypeService transactionTypeService)
    : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;
    private readonly ICustomerRepository _customerRepository = unitOfWork.CustomerRepository;
    private readonly ITransactionTypeService _transactionTypeService = transactionTypeService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<TransactionService> _logger = logger;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(
        CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get all transactions");
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

    public async Task<(decimal balance, Transaction transactionDetail, TenantDto tenantDto)>
        PostTransactionAsync(
            string customerId,
            string tenantId,
            string slug,
            CustomerPostTransaction dto,
            CancellationToken cancellationToken = default)
    {
        var validator = new CustomerPostTransactionValidator(_customerRepository);
        var result = await validator.ValidateAsync(dto,cancellationToken);
        
        if (!result.IsValid)
            throw new BadHttpRequestException(string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
        
        var performBy = _currentUserProvider.UserId;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Tenant: {TenantId} perform {slug} operation to Customer: {CustomerId}, perform by user {PerformBy}.",
                tenantId, slug, customerId, performBy);

        return await ExecuteWithRetryAsync(async () =>
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var type = await _transactionTypeService.GetBySlugAsync(slug, cancellationToken);
                if (type == null)
                    throw new BadHttpRequestException($"Invalid Transaction Type");

                if (!type.AllowNegative && dto.Amount < 0)
                    throw new BadHttpRequestException(
                        $"Negative amounts are not allowed for transaction type '{type.Name}'.");
                
                var finalAmount = dto.Amount * type.Multiplier;
                var (customer, tenant) = await GetValidCustomerAndTenantAsync(customerId, tenantId, cancellationToken);
                
                var account = customer.Accounts.FirstOrDefault(e => e.TenantId == tenant.Id);

                if (account is null)
                {
                    if (finalAmount > 0)
                    {
                        account = customer.CreateAccount(tenantId);
                        if (_logger.IsEnabled(LogLevel.Information))
                            _logger.LogInformation(
                                "Account was create for customer {CustomerId} and tenant {TenantId}",
                                customerId, tenantId);
                        
                        // Save the state if the context won't save it
                        // await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        // If customer doesn't account with tenant, and the first amount is not positive,
                        // It won't trigger the account creation
                        throw new BadHttpRequestException(
                            $"No account was found for customer: {customerId}, and tenant: {tenantId}");
                    }
                }

                // Process the Transaction (Generic form, support all transaction type)
                var (balance, transactionDetail) = account.ProcessTransaction(
                    finalAmount,
                    type.Id,
                    dto.Reason,
                    dto.ReferenceId,
                    performBy,
                    dto.OccurredAt);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation(
                        "Transaction was created with id: {TransactionId}, with Reason {Reason}.",
                        transactionDetail.Id, dto.Reason);
                var tenantDto = _mapper.Map<TenantDto>(tenant);
                return (balance, transactionDetail, tenantDto);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries = 3)
    {
        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                return await action();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (i == maxRetries - 1) throw;
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning(ex, "Concurrency conflict detected. Retrying... Attempt {Attempt}", i + 1);
                
                // Backoff
                await Task.Delay(100);
            }
        }
        throw new InvalidOperationException("Should never be reached");
    }
}