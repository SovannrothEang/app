namespace Application.DTOs.Customers;

public record PostTransactionDto(
    decimal Amount,
    string? Reason,
    string? ReferenceId,
    string? IdempotencyKey,
    DateTimeOffset? OccurredAt);