namespace Application.DTOs.Customers;

public record CustomerCreateDto(
    string? Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password,
    string ConfirmPassword);