using CoreAPI.DTOs.Auth;

namespace CoreAPI.DTOs.Tenants;
public record TenantCreateDto(string Name, int PointPerDollar, int ExpiryDays, RegisterDto RegisterDto);
