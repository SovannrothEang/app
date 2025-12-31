using CoreAPI.DTOs.Customers;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken ct = default);
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
    
    Task<IEnumerable<Transaction>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default);
    
    Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        CancellationToken cancellationToken = default);
    
    Task<(decimal balance, Transaction transactionDetail)> EarnPointAsync(
        string customerId,
        string tenantId,
        CustomerEarnPointDto dto,
        CancellationToken cancellationToken = default);
    
    Task<(decimal balance, Transaction transactionDetail)> RedeemPointAsync(
        string customerId,
        string tenantId,
        CustomerRedeemPointDto dto,
        CancellationToken cancellationToken = default);
    
    Task<(decimal balance, Transaction transactionDetail)> AdjustPointAsync(
        string customerId,
        string tenantId,
        string? performBy,
        CustomerAdjustPointDto dto,
        CancellationToken cancellationToken = default);
}