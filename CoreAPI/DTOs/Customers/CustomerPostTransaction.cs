namespace CoreAPI.DTOs.Customers;

public record CustomerPostTransaction(
    decimal Amount,
    string? Reason,
    string? ReferenceId,
    DateTimeOffset? OccurredAt);