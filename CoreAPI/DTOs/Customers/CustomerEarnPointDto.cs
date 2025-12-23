namespace CoreAPI.DTOs.Customers;

public record CustomerEarnPointDto(
    int Amount,
    string? Reason);