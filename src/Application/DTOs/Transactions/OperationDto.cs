namespace Application.DTOs.Transactions;

public record OperationDto(
    string Slug,
    string Name,
    string? Description,
    string Url);