using CoreAPI.DTOs.Customers;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<CustomerDto>> GetAllWithIncludesAsync(CancellationToken ct = default);
    Task<CustomerDto?> GetByIdAsync(string id, bool? childIncluded = false, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken ct = default);
    
    Task<CustomerDto> GetValidCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default);

    Task<(decimal balance, List<PointTransaction> list)> GetCustomerBalanceByIdAsync(
        string customerId,
        string tenantId,
        CustomerGetBalanceOptionsDto? options,
        CancellationToken cancellationToken = default);
    
    Task<(Customer customer, Tenant tenant)> ValidCustomerAndTenantAsync(
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