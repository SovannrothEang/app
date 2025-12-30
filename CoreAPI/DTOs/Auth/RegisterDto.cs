namespace CoreAPI.DTOs.Auth;

public record RegisterDto(string UserName, string Email, string Password, string ConfirmPassword);
