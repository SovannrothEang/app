namespace Application.DTOs.Transactions;

public record TransactionTypeCreateDto(
    string Slug,
    string Name,
    string? Description = null,
    int Multiplier = 1,
    bool AllowNegative = false)
{
    public TransactionTypeCreateDto() : this(
        string.Empty,
        string.Empty) { }
}