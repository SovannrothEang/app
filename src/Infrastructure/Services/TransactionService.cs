using Application.DTOs.Customers;
using Application.DTOs.Tenants;
using Application.DTOs.Transactions;
using Application.Services;
using Application.Validators.Customers;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Infrastructure.Services;

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
    private readonly IRepository<Transaction> _transactionRepository = unitOfWork.GetRepository<Transaction>();
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
            _logger.LogInformation("[TransactionService] Get all transactions as page result by User {Performer}", _currentUserProvider.UserId);

        var (items, totalCount) = await _transactionRepository.GetPagedResultAsync(
            option,
            ignoreQueryFilters: true,
            filter: BuildTransactionFilter(option),
            includes: BuildTransactionIncludes(childIncluded),
            orderBy: BuildTransactionOrderBy(option),
            cancellationToken: ct);
        return new PagedResult<TransactionDto>()
        {
            Items = [..items.Select(_mapper.Map<TransactionDto>)],
            PageNumber = option.Page.Value,
            PageSize = option.PageSize.Value,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<TransactionDto>> GetAllByCustomerIdForTenantAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        pageOption.Page ??= 1;
        pageOption.PageSize ??= 10;

        var baseFilter = BuildTransactionFilter(pageOption);
        Expression<Func<Transaction, bool>> customerFilter = e => e.CustomerId == customerId;
        var combinedFilter = CombineFilters(customerFilter, baseFilter);

        var (items, totalCount) = await _transactionRepository.GetPagedResultAsync(
            pageOption,
            ignoreQueryFilters: false,
            filter: combinedFilter,
            includes: BuildTransactionIncludes(childIncluded),
            orderBy: BuildTransactionOrderBy(pageOption),
            cancellationToken: cancellationToken);
        return new PagedResult<TransactionDto>()
        {
            Items = [.. items.Select(_mapper.Map<TransactionDto>)],
            PageNumber = pageOption.Page.Value,
            PageSize = pageOption.PageSize.Value,
            TotalCount = totalCount
        };
    }
    
    public async Task<PagedResult<TransactionDto>> GetAllByCustomerIdForGlobalAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        pageOption.Page ??= 1;
        pageOption.PageSize ??= 10;

        var baseFilter = BuildTransactionFilter(pageOption);
        Expression<Func<Transaction, bool>> customerFilter = e => e.CustomerId == customerId;
        var combinedFilter = CombineFilters(customerFilter, baseFilter);

        var (items, totalCount) = await _transactionRepository.GetPagedResultAsync(
            pageOption,
            ignoreQueryFilters: true,
            filter: combinedFilter,
            includes: BuildTransactionIncludes(childIncluded),
            orderBy: BuildTransactionOrderBy(pageOption),
            cancellationToken: cancellationToken);
        return new PagedResult<TransactionDto>()
        {
            Items = [.. items.Select(_mapper.Map<TransactionDto>)],
            PageNumber = pageOption.Page.Value,
            PageSize = pageOption.PageSize.Value,
            TotalCount = totalCount
        };
    }

    public async Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "[TransactionService] Validating Customer: {CustomerId} and Tenant: {TenantId}, get by User: {Performer}",
                customerId, tenantId, _currentUserProvider.UserId);
        var tenant = await _unitOfWork.GetRepository<Tenant>().FirstOrDefaultAsync(
            predicate: e => e.Id == tenantId && e.Status == TenantStatus.Active,
            trackChanges: trackChanges,
            cancellationToken: cancellationToken);
        if (tenant is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No active Tenant was found with id: {TenantId}.", tenantId);
            throw new KeyNotFoundException($"No Tenant was found with id: {tenantId}.");
        }
        var customer = await _unitOfWork.GetRepository<Customer>().FirstOrDefaultAsync(
            predicate: e => e.Id == customerId,
            trackChanges: trackChanges,
            ignoreQueryFilters: true,
            includes: queryable => queryable
                .Include(c => c.User)
                .Include(c => c.Accounts)
                    .ThenInclude(acc => acc.AccountType),
            cancellationToken: cancellationToken);
        if (customer is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No active Tenant was found with id: {TenantId}.", tenantId);
            throw new KeyNotFoundException($"No Customer was found with id: {customerId}.");
        }

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
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning(
                    "PostTransactionAsync validation failed: {Errors}", errors);
            throw new BadHttpRequestException(errors);
        }

        var performBy = _currentUserProvider.UserId;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Tenant: {TenantId} perform {Slug} operation to Customer: {CustomerId}, perform by User {PerformBy}.",
                tenantId, slug, customerId, performBy);

        return await ExecuteWithRetryAsync(async () =>
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var type = await _transactionTypeService.GetBySlugAsync(slug, cancellationToken);
                if (!type.AllowNegative && dto.Amount < 0)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning(
                            "Negative amounts are not allowed for transaction type: {TypeName}", type.Name);
                    throw new BadHttpRequestException(
                        $"Negative amounts are not allowed for transaction type '{type.Name}'.");
                }

                var finalAmount = dto.Amount * type.Multiplier;
                var (customer, tenant) = await GetValidCustomerAndTenantAsync(
                    customerId, tenantId, trackChanges: true, cancellationToken);

                var account = customer.Accounts.FirstOrDefault(e =>
                    e.TenantId == tenant.Id &&
                    e.AccountTypeId == accountTypeId);

                if (account is null)
                {
                    if (finalAmount > 0 && await _accountTypeService.ExistsAsync(accountTypeId, cancellationToken))
                    {
                        account = customer.CreateAccount(tenantId, accountTypeId);
                        if (_logger.IsEnabled(LogLevel.Information))
                            _logger.LogInformation(
                                "Account was create for customer {CustomerId} and tenant {TenantId}",
                                customerId, tenantId);
                    }
                    else
                    {
                        // If customer doesn't account with tenant, and the first amount is not positive,
                        // It won't trigger the account creation
                        if (_logger.IsEnabled(LogLevel.Warning))
                            _logger.LogWarning(
                                "No account was found for customer: {CustomerId}, and tenant: {TenantId}, with type of account: {AccountTypeId}",
                                customerId, tenantId, accountTypeId);
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
    
    /// <summary>
    /// Builds the filter expression for transaction queries based on pagination options.
    /// Supports filtering by: id, tenantid, customerid, accounttypeid, type (TransactionType name), occurredat.
    /// </summary>
    private static Expression<Func<Transaction, bool>>? BuildTransactionFilter(PaginationOption option)
    {
        if (string.IsNullOrEmpty(option.FilterBy) || string.IsNullOrEmpty(option.FilterValue))
            return null;

        return option.FilterBy.ToLower() switch
        {
            "id" => e => e.Id == option.FilterValue,
            "tenantid" => e => e.TenantId == option.FilterValue,
            "customerid" => e => e.CustomerId == option.FilterValue,
            "accounttypeid" => e => e.AccountTypeId == option.FilterValue,
            "type" => e => e.TransactionType!.Name == option.FilterValue,
            "occurredat" => DateTime.TryParse(option.FilterValue, out var occurredAt)
                ? e => e.OccurredAt.Date == occurredAt.Date
                : null,
            _ => throw new BadHttpRequestException($"Filtering by '{option.FilterBy}' is not supported.")
        };
    }

    /// <summary>
    /// Builds the includes for transaction queries.
    /// Always includes TransactionType. Optionally includes Referencer and Performer.
    /// </summary>
    private static Func<IQueryable<Transaction>, IQueryable<Transaction>> BuildTransactionIncludes(bool childIncluded)
    {
        return queryable =>
        {
            queryable = queryable.Include(e => e.TransactionType);
            if (childIncluded)
            {
                queryable = queryable
                    .Include(e => e.Referencer)
                    .Include(e => e.Performer);
            }
            return queryable;
        };
    }

    /// <summary>
    /// Builds the order by expression for transaction queries.
    /// Supports sorting by: balance (Amount), type, occurredat, createdat (default).
    /// </summary>
    private static Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>> BuildTransactionOrderBy(PaginationOption option)
    {
        var sortBy = (option.SortBy ?? "createdAt").ToLower();
        var sortDirection = (option.SortDirection ?? "asc").ToLower();

        return (sortBy, sortDirection) switch
        {
            ("balance", "asc") => q => q.OrderBy(x => x.Amount),
            ("balance", "desc") => q => q.OrderByDescending(x => x.Amount),
            ("type", "asc") => q => q
                .OrderBy(x => x.TransactionType!.Name)
                .ThenBy(x => x.OccurredAt),
            ("type", "desc") => q => q
                .OrderByDescending(x => x.TransactionType!.Name)
                .ThenBy(x => x.OccurredAt),
            ("occurredat", "asc") => q => q
                .OrderBy(x => x.OccurredAt)
                .ThenBy(x => x.TransactionType!.Name),
            ("occurredat", "desc") => q => q
                .OrderByDescending(x => x.OccurredAt)
                .ThenBy(x => x.TransactionType!.Name),
            ("createdat", "asc") => q => q
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.TransactionType!.Name),
            ("createdat", "desc") => q => q
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.TransactionType!.Name),
            _ => q => q
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.TransactionType!.Name)
        };
    }

    /// <summary>
    /// Combines two filter expressions with AND logic.
    /// </summary>
    private static Expression<Func<Transaction, bool>> CombineFilters(
        Expression<Func<Transaction, bool>> first,
        Expression<Func<Transaction, bool>>? second)
    {
        if (second is null)
            return first;

        var parameter = Expression.Parameter(typeof(Transaction), "e");
        var firstBody = ReplaceParameter(first.Body, first.Parameters[0], parameter);
        var secondBody = ReplaceParameter(second.Body, second.Parameters[0], parameter);
        var combined = Expression.AndAlso(firstBody, secondBody);
        return Expression.Lambda<Func<Transaction, bool>>(combined, parameter);
    }

    /// <summary>
    /// Replaces parameter in expression tree.
    /// </summary>
    private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
    {
        return new ParameterReplacer(oldParam, newParam).Visit(expression);
    }

    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam = oldParam;
        private readonly ParameterExpression _newParam = newParam;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParam ? _newParam : base.VisitParameter(node);
        }
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
    #endregion
}
