namespace Application.DTOs.Auth;

public record OnboardingUserDto(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string? Role = null);
