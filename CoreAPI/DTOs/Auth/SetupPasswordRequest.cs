using System.ComponentModel.DataAnnotations;

namespace CoreAPI.DTOs.Auth;

public record SetupPasswordRequest(
    [Required] string UserId,
    [Required] [EmailAddress] string Email,
    [Required] string Token,
    [Required] [MinLength(8)] string NewPassword);