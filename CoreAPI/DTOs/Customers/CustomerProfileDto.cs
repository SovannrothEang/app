using CoreAPI.DTOs.Transactions;

namespace CoreAPI.DTOs.Customers;

public record CustomerProfileDto(
    string Id,
    string Name,
    string Email,
    string PhoneNumber,
    decimal TotalBalance,
    IReadOnlyList<TenantProfileDto> Tenants);
    
public record TenantProfileDto(
    string TenantId,
    string TenantName,
    decimal TotalBalance,
    IReadOnlyList<AccountCustomerProfileDto> Accounts);
    
public record AccountCustomerProfileDto(
    string Type,
    decimal Balance,
    // IReadOnlyList<TransactionDto> Customers,
    PagedResult<TransactionDto> Transactions,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);