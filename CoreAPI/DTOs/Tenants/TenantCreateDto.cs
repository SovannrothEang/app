namespace CoreAPI.DTOs.Tenants;
public record TenantCreateDto(TenantCreate Tenant, TenantOwnerCreate Owner);

public record TenantCreate(string Name, int PointPerDollar, int ExpiryDays);
public record TenantOwnerCreate(string UserName, string Email, string FirstName, string LastName);
