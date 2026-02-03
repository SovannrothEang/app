namespace Application.DTOs.Auth;

/// <summary>
/// This DTO is used for setting up a new password during user onboarding or password reset processes. (Development ONLY)
/// </summary>
public record SetupPasswordRequest(
    string UserId,
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword);