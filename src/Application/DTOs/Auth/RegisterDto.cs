namespace Application.DTOs.Auth;

public record RegisterDto(string UserName, string Email,string FirstName, string LastName, string Password, string ConfirmPassword);