namespace CoreAPI.DTOs.Customers;

public record CustomerCreateDto(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password,
    string ConfirmPassword);