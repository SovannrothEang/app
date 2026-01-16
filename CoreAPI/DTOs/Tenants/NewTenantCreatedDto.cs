namespace CoreAPI.DTOs.Tenants;

public record NewTenantCreatedDto(
    TenantDto Tenant,
    string Token);