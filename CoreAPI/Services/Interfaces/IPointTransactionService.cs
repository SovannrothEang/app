using CoreAPI.DTOs.Customers;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface IPointTransactionService
{
    Task<IEnumerable<PointTransaction>> GetAllTransactionsAsync(CancellationToken ct = default);
    Task<IEnumerable<PointTransaction>> GetAllByTenantAndCustomerAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken = default);
    
    Task<PointTransaction?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);
    
    Task<(Customer customer, Tenant tenant)> GetValidCustomerAndTenantAsync(
        string customerId,
        string tenantId,
        CancellationToken cancellationToken = default);
    
    Task<(decimal balance, PointTransaction transactionDetail)> EarnPointAsync(
        string customerId,
        string tenantId,
        CustomerEarnPointDto dto,
        CancellationToken cancellationToken = default);
    
    Task<(decimal balance, PointTransaction transactionDetail)> RedeemPointAsync(
        string customerId,
        string tenantId,
        CustomerRedeemPointDto dto,
        CancellationToken cancellationToken = default);
    
    Task<(decimal balance, PointTransaction transactionDetail)> AdjustPointAsync(
        string customerId,
        string tenantId,
        string? performBy,
        CustomerAdjustPointDto dto,
        CancellationToken cancellationToken = default);
}