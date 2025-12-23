namespace CoreAPI.DTOs.Customers;

public record CustomerCreateDto(
    string Name,
    string Email,
    string PhoneNumber);