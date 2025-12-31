using CoreAPI.Models;

namespace CoreAPI.DTOs.Customers;

public record CustomerDto(
    string Id,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    IList<Account> LoyaltyAccounts,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
