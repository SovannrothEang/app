using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;
using CoreAPI.Models.Enums;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using CoreAPI.Validators.Customers;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Services;

public class TransactionService(
    IUnitOfWork unitOfWork,
    ILogger<TransactionService> logger,
    ICurrentUserProvider currentUserProvider,
    ITransactionTypeService transactionTypeService,
    IAccountTypeService accountTypeService,
    IMapper mapper)
    : ITransactionService
{
    #region Private fields
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
    private readonly ITransactionTypeService _transactionTypeService = transactionTypeService;
    private readonly IAccountTypeService _accountTypeService = accountTypeService;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<TransactionService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    #endregion

    // Global admin access
    public async Task<PagedResult<TransactionDto>> GetPagedResultAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Get all transactions");

        var (transactions, totalCount) = await _transactionRepository.GetAllForGlobalAsync(
            option,
            childIncluded: childIncluded,
            cancellationToken: ct);

        var dtos = transactions.Select(t => _mapper.Map<TransactionDto>(t)).ToList();

        return new PagedResult<TransactionDto>
        {
            Items = dtos,
            PageNumber = option.Page!.Value,
            PageSize = option.PageSize!.Value,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<TransactionDto>> GetAllByCustomerIdForTenantAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var (result, totalCount) = await _transactionRepository.GetAllByCustomerIdAsync(
            customerId,
            pageOption,
            childIncluded,
            cancellationToken);

        return new PagedResult<TransactionDto>
        {
            Items = [.. result.Select(x => _mapper.Map<TransactionDto>(x))],
            PageNumber = pageOption.Page!.Value,
            PageSize = pageOption.PageSize!.Value,
            TotalCount = totalCount
        };
    }
    
    public async Task<PagedResult<TransactionDto>> GetAllByCustomerIdForGlobalAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        var (result, totalCount) = await _transactionRepository.GetAllByCustomerIdForGlobalAsync(
            customerId,
            pageOption,
            childIncluded,
            cancellationToken);

        return new PagedResult<TransactionDto>
        {
            Items = [.. result.Select(x => _mapper.Map<TransactionDto>(x))],
            PageNumber = pageOption.Page!.Value,
            PageSize = pageOption.PageSize!.Value,
            TotalCount = totalCount
        };
    }

    public async Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.GetRepository<Tenant>().FirstOrDefaultAsync(
            predicate: e => e.Id == tenantId && e.Status == TenantStatus.Active,
            trackChanges: trackChanges,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"No Tenant was found with id: {tenantId}.");

        var customer = await _unitOfWork.GetRepository<Customer>().FirstOrDefaultAsync(
            predicate: e => e.Id == customerId,
            trackChanges: trackChanges,
            ignoreQueryFilters: true,
            includes: queryable => queryable
                .Include(c => c.User)
                .Include(c => c.Accounts)
                    .ThenInclude(acc => acc.AccountType),
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"No Customer was found with id: {customerId}.");

        return (customer, tenant);
    }

    public async Task<(decimal balance, TransactionDto transactionDetail, TenantDto tenantDto)>
        PostTransactionAsync(
            string customerId,
            string tenantId,
            string accountTypeId,
            string slug,
            PostTransactionDto dto,
            CancellationToken cancellationToken = default)
    {
        var validator = new PostTransactionValidator(_unitOfWork);
        var result = await validator.ValidateAsync(dto, cancellationToken);

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
                var type = await _transactionTypeService.GetBySlugAsync(slug, cancellationToken)
                    ?? throw new BadHttpRequestException("Invalid Transaction Type, get available type with: /api/transactions/{transactionId}/operations");

                if (!type.AllowNegative && dto.Amount < 0)
                    throw new BadHttpRequestException(
                        $"Negative amounts are not allowed for transaction type '{type.Name}'.");

                var finalAmount = dto.Amount * type.Multiplier;
                var (customer, tenant) = await GetValidCustomerAndTenantAsync(
                    customerId, tenantId, trackChanges: true, cancellationToken);

                var account = customer.Accounts.FirstOrDefault(e =>
                    e.TenantId == tenant.Id &&
                    e.AccountTypeId == accountTypeId);

                if (account is null)
                {
                    // if (finalAmount > 0 && await _accountTypeRepository.ExistsAsync(accountTypeId, cancellationToken))
                    if (finalAmount > 0 && await _accountTypeService.ExistsAsync(accountTypeId, cancellationToken))
                    {
                        account = customer.CreateAccount(tenantId, accountTypeId);
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
                            $"No account was found for customer: {customerId}, and tenant: {tenantId}, with type of account: {accountTypeId}");
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

                await _unitOfWork.CompleteAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation(
                        "Transaction was created with id: {TransactionId}, with Reason {Reason}.",
                        transactionDetail.Id, dto.Reason);
                var tenantDto = _mapper.Map<TenantDto>(tenant);
                return (balance, _mapper.Map<TransactionDto>(transactionDetail), tenantDto);
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

    #region Helper Methods
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
    #endregion
}