using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ITransactionService
{
    // Task<IEnumerable<TransactionDto>> GetAllAsync(
    //     PaginationOption option,
    //     bool childIncluded = false,
    //     CancellationToken ct = default);
    
    /// <summary>
    /// Get all transactions with pagination, for Global user
    /// </summary>
    /// <param name="option"></param>
    /// <param name="childIncluded"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<PagedResult<TransactionDto>> GetPagedResultAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default);

    /// <summary>
    /// Get all transactions by customerId for Tenant user
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="pageOption"></param>
    /// <param name="childIncluded"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedResult<TransactionDto>> GetAllByCustomerIdForTenantAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all transactions by customerId for Global user
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="pageOption"></param>
    /// <param name="childIncluded"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedResult<TransactionDto>> GetAllByCustomerIdForGlobalAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get valid customer and tenant by their IDs
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="tenantId"></param>
    /// <param name="trackChanges"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Post a transaction for a customer under a tenant and account type
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="tenantId"></param>
    /// <param name="accountTypeId"></param>
    /// <param name="slug"></param>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(decimal balance, TransactionDto transactionDetail, TenantDto tenantDto)>
        PostTransactionAsync(
            string customerId,
            string tenantId,
            string accountTypeId,
            string slug,
            PostTransactionDto dto,
            CancellationToken cancellationToken = default);
}