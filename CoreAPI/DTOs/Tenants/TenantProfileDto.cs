using CoreAPI.DTOs.Accounts;

namespace CoreAPI.DTOs.Tenants;

public record TenantProfileDto(
    string TenantId,
    string TenantName,
    decimal TotalBalance,
    PagedResult<AccountProfileDto> Accounts);
