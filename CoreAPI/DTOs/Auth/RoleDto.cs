namespace CoreAPI.DTOs.Auth;

public record RoleDto(
    string Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
