namespace CoreAPI.DTOs.Customers;

public record PostTransactionDto(
    decimal Amount,
    string? Reason,
    string? ReferenceId,
    DateTimeOffset? OccurredAt);