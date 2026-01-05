using CoreAPI.DTOs.Auth;

namespace CoreAPI.DTOs.Tenants;
public record TenantCreateDto(TenantCreate Tenant, TenantOwnerCreate Owner);

public abstract record TenantCreate(string Name, int PointPerDollar, int ExpiryDays);
public abstract record TenantOwnerCreate(string UserName, string Email);
