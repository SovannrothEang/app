namespace CoreAPI.DTOs.Customers;

public record CustomerUpdateDto(
    string? Name,
    string? Email,
    string? PhoneNumer);