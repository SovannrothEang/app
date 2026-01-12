using CoreAPI.DTOs.Accounts;
using CoreAPI.DTOs.Customers;

namespace CoreAPI.DTOs.Transactions;

public record TransactionDto(
    string Id,
    string TenantId,
    string CustomerId,
    string AccountTypeId,
    AccountTypeDto? AccountType,
    decimal Amount,
    string TransactionTypeId,
    TransactionTypeDto? TransactionType,
    string? Reason,
    string? ReferenceId,
    CustomerDto? Customer,
    string? PerformBy,
    UserProfileDto? Performer,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt)
{
    public TransactionDto() : this(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        0,
        string.Empty,
        null,
        null,
        null,
        null,
        null,
        null,
        new DateTimeOffset(),
        new DateTimeOffset()) { }
}