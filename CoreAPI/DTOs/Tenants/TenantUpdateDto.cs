namespace CoreAPI.DTOs.Tenants;

// public record TenantUpdateDto(string Name, LoyaltyProgramSetting Setting);
public record TenantUpdateDto(string? Name, int? PointPerDollar, int? ExpiryDays);
