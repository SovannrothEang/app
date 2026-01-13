using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ITransactionService
{
    Task<PagedResult<TransactionDto>> GetAllTransactionsAsync(PaginationOption option, bool childIncluded = false, CancellationToken ct = default);
    Task<IEnumerable<TransactionDto>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken = default);
    Task<PagedResult<TransactionDto>> GetAllByCustomerAsync(
        string customerId,
        PaginationOption pageOption,
        bool childIncluded,
        CancellationToken cancellationToken = default);
    Task<TransactionDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TransactionDto>> GetByTenantIdAsync(
        string tenantId,
        CancellationToken ct = default);

    Task<IEnumerable<TransactionDto>> GetByCustomerIdForTenantAsync(
        string customerId,
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