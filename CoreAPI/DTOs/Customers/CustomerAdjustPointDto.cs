namespace CoreAPI.DTOs.Customers;

public record CustomerAdjustPointDto(
    int Amount,
    string? Reason);