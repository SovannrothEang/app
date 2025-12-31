using System.ComponentModel.DataAnnotations;

namespace CoreAPI.DTOs.Auth;

public record SetupPasswordRequest(
    string UserId,
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword);