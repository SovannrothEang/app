using CoreAPI.Models;
using CoreAPI.Models.Enums;

namespace CoreAPI.DTOs.Accounts;

public record AccountDto(
    string TenantId,
    string CustomerId,
    int Balance,
    TierLevel Tier,
    IReadOnlyList<Transaction> PointTransactions);