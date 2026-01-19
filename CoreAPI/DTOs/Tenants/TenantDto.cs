using CoreAPI.Models;
using CoreAPI.Models.Enums;

namespace CoreAPI.DTOs.Tenants;

public record TenantOnboardResponseDto(
    TenantDto Tenant,
    string UserId,
    string Token);

public record TenantDto(
    string Id,
    string Name,
    TenantStatus Status,
    AccountSetting? Setting,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);