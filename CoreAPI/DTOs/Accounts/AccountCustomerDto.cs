using CoreAPI.Models;
using CoreAPI.Models.Enums;

namespace CoreAPI.DTOs.Accounts;

public record AccountCustomerDto(
    int CurrentBalance,
    TierLevel Tier,
    Transaction? LastTransaction);
