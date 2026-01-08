namespace CoreAPI.DTOs.Transactions;

public record TransactionTypeDto(
    string Id,
    string Slug,
    string Name,
    string? Description,
    int Multiplier = 1,
    bool AllowNegative = false)
{
    public TransactionTypeDto() : this(
        Guid.NewGuid().ToString(),
        string.Empty,
        string.Empty,
        null) {}
}