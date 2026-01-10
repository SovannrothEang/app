using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ITransactionService
{
    Task<PagedResult<TransactionDto>> GetAllTransactionsAsync(PaginationOption option, bool childIncluded = false, CancellationToken ct = default);
    Task<IEnumerable<Transaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetAllByCustomerAsync(string customerId, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetByTenantIdAsync(
        string tenantId,
        CancellationToken ct = default);
    
    Task<IEnumerable<Transaction>> GetByCustomerIdForTenantAsync(
        string customerId,
        CancellationToken cancellationToken = default);
    
    Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        CancellationToken cancellationToken = default);
    
    Task<(decimal balance, Transaction transactionDetail, TenantDto tenantDto)> PostTransactionAsync(
        string customerId,
        string tenantId,
        string slug,
        CustomerPostTransaction dto,
        CancellationToken cancellationToken = default);
}