using CoreAPI.DTOs;
using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Tenants;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Services.Interfaces;

public interface IUserService
{
    Task<(string userId, string token)> OnboardingUserAsync(OnboardingUserDto dto, CancellationToken ct = default);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default);

    Task<UserProfileDto> CreateUserAsync(RegisterDto dto, CancellationToken ct = default);
    Task<UserProfileDto> GetCurrentUserProfileAsync(string userId);
    Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    //Task SendPasswordResetEmailAsync(string email);
    Task CompleteInviteAsync(string userId, string token, string newPassword);
    Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
    Task<(string userId, string token)> CreateTenantAndUserAsync(TenantCreateDto dto, CancellationToken ct = default);

    Task<IEnumerable<UserProfileDto>> GetAllUserAsync(CancellationToken ct = default);
    Task<UserProfileDto?> GetUserById(string id, CancellationToken ct = default);

    // Email verification
    //Task SendEmailVerificationEmailAsync(User user);
    //Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
    //Task ResendVerificationEmailAsync(string email);

    // Account management
    //Task<IdentityResult> ChangeEmailAsync(string userId, string newEmail);
    //Task<IdentityResult> ConfirmEmailChangeAsync(string userId, string token);
    //Task<IdentityResult> DeleteAccountAsync(string userId, string? password = null);
}
