using CoreAPI.DTOs.Tenants;

namespace CoreAPI.DTOs.Customers;

public record CustomerProfileDto(
    string Id,
    string Name,
    string Email,
    string PhoneNumber,
    decimal TotalBalance,
    IReadOnlyList<TenantProfileDto> Tenants);
