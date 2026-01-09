using CoreAPI.Models;
using CoreAPI.Models.Enums;

namespace CoreAPI.DTOs.Accounts;

public record AccountDto(
    string TenantId,
    string CustomerId,
    decimal Balance,
    IReadOnlyList<Transaction> Transactions,
    string? PerformBy,
    UserProfileDto? Performer)
{
    public AccountDto() : this(
        string.Empty,
        string.Empty,
        0,
        [],
        null,
        null) { }
}