using Application.DTOs.Transactions;

namespace Application.DTOs.Accounts;

public record AccountProfileDto(
    string AccountTypeId,
    AccountTypeDto? AccountType,
    decimal Balance,
    IReadOnlyList<TransactionDto> Transactions,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);