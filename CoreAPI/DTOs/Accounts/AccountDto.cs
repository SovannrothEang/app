using CoreAPI.DTOs.Tenants;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.DTOs.Accounts;

public record AccountDto(
    string TenantId,
    string CustomerId,
    string  AccountTypeId,
    AccountTypeDto? AccountType,
    decimal Balance,
    IReadOnlyList<TransactionDto> Transactions,
    TenantDto? Tenant,
    string? PerformBy,
    UserProfileDto? Performer,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public AccountDto() : this(
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        0,
        [],
        null,
        null,
        null,
        new DateTimeOffset(),
        null) { }
}