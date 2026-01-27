namespace CoreAPI.DTOs.Auth;

public record UserCreateDto(
    string? Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string RoleName,
    string Password,
    string ConfirmPassword);
