using CoreAPI.Models;
using CoreAPI.Models.Enums;

namespace CoreAPI.DTOs.Tenants;

public record TenantDto(
    string Id,
    string Name,
    TenantStatus Status,
    LoyaltyProgramSetting Setting,
    DateTime CreatedAt,
    DateTime? UpdatedAt);