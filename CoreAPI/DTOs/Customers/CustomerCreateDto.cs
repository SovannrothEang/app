namespace CoreAPI.DTOs.Customers;

public record CustomerCreateDto(
    string UserName,
    string Email,
    string PhoneNumber,
    string Password);