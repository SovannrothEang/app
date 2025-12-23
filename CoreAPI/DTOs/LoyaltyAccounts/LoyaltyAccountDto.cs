using CoreAPI.Models;
using CoreAPI.Models.Enums;

namespace CoreAPI.DTOs.LoyaltyAccounts;

public record LoyaltyAccountDto(
    string TenantId,
    string CustomerId,
    int Balance,
    TierLevel Tier,
    IReadOnlyList<PointTransaction> PointTransactions);