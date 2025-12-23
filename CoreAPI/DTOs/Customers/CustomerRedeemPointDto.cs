namespace CoreAPI.DTOs.Customers;

public record CustomerRedeemPointDto(
    int Amount,
    string? Reason);