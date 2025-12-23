using CoreAPI.DTOs;
using CoreAPI.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Services.Interfaces;

public interface IUserService
{
    Task<IdentityResult> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserProfileResponseDto> GetCurrentUserProfileAsync(string userId);
    Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordCommand command);
    //Task SendPasswordResetEmailAsync(string email);
    Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);

    Task<IEnumerable<UserProfileResponseDto>> GetAllUserAsync(CancellationToken ct = default);
    Task<UserProfileResponseDto?> GetUserById(string id, CancellationToken ct = default);

    // Email verification
    //Task SendEmailVerificationEmailAsync(User user);
    //Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
    //Task ResendVerificationEmailAsync(string email);

    // Account management
    //Task<IdentityResult> ChangeEmailAsync(string userId, string newEmail);
    //Task<IdentityResult> ConfirmEmailChangeAsync(string userId, string token);
    //Task<IdentityResult> DeleteAccountAsync(string userId, string? password = null);
}
