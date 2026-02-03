namespace Application.DTOs.Tenants;
public record TenantOnBoardingDto(TenantCreateDto Tenant, TenantOwnerCreateDto Owner);

public record TenantCreateDto(string Name, int? PointPerDollar, int? ExpiryDays);
public record TenantOwnerCreateDto(string UserName, string Email, string FirstName, string LastName);
