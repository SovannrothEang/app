namespace Application.DTOs
{
    public record AuthResponseDto(
        string AccessToken,
        DateTimeOffset ExpiresAt,
        string RefreshToken,
        string UserId,
        string Email,
        IEnumerable<string> Roles
    );

    public record UserProfileDto(
        string Id, string? UserName, string? Email, IList<string> Roles
    );
}
