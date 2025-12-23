namespace CoreAPI.DTOs.Customers;

public record CustomerGetBalanceOptionsDto(
    string? TransactionType,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Pagination? Pagination = null);