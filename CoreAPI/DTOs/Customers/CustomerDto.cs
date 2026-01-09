using CoreAPI.DTOs.Accounts;
using CoreAPI.Models;

namespace CoreAPI.DTOs.Customers;

public record CustomerDto(
    string Id,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    IList<AccountDto> Accounts,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public CustomerDto() : this(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        [],
        new DateTimeOffset(),
        null) { }
}
