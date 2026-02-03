using Application.DTOs.Accounts;
using Domain.Shared;

namespace Application.DTOs.Tenants;

public record TenantProfileDto(
    string TenantId,
    string TenantName,
    decimal TotalBalance,
    PagedResult<AccountProfileDto> Accounts);
