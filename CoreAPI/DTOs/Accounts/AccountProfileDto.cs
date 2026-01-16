using CoreAPI.DTOs.Transactions;

namespace CoreAPI.DTOs.Accounts;

public record AccountProfileDto(
    string AccountTypeId,
    AccountTypeDto? AccountType,
    decimal Balance,
    IReadOnlyList<TransactionDto> Transactions,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);