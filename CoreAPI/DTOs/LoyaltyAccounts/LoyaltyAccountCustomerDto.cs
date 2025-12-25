using CoreAPI.Models;
using CoreAPI.Models.Enums;

namespace CoreAPI.DTOs.LoyaltyAccounts;

public record LoyaltyAccountCustomerDto(
    int CurrentBalance,
    TierLevel Tier,
    PointTransaction? LastTransaction);
