using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ITransactionService
{
    /// <summary>
    /// Get all transactions 
    /// </summary>
    /// <param name="option"></param>
    /// <param name="childIncluded"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<PagedResult<TransactionDto>> GetAllAsync(
        PaginationOption option,
        bool childIncluded = false,
        CancellationToken ct = default);
    
    Task<PagedResult<TransactionDto>> GetAllByCustomerIdForGlobalAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded,
        CancellationToken cancellationToken = default);

    Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    Task<(decimal balance, TransactionDto transactionDetail, TenantDto tenantDto)> PostTransactionAsync(
        string customerId,
        string tenantId,
        string accountTypeId,
        string slug,
        PostTransactionDto dto,
        CancellationToken cancellationToken = default);
}