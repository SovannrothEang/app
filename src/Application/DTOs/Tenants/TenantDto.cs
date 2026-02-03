using Application.DTOs.Accounts;
using Domain.Entities;
using Domain.Enums;

namespace Application.DTOs.Tenants;

public record TenantOnboardResponseDto(
    TenantDto Tenant,
    string UserId,
    string Token);

public record TenantDto(
    string Id,
    string Name,
    TenantStatus Status,
    AccountSetting? Setting,
    IReadOnlyList<AccountTypeDto> AccountTypes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public TenantDto() : this(
        Id: string.Empty,
        Name: string.Empty,
        Status: TenantStatus.Inactive,
        Setting: null,
        AccountTypes: [],
        CreatedAt: DateTimeOffset.MinValue,
        UpdatedAt: null) { }
}