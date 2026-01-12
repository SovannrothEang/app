namespace CoreAPI.DTOs.Accounts;

public record AccountTypeDto(
    string Id, 
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);