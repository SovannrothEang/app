namespace CoreAPI.DTOs.Tenants;

public record TenantLevelDto(string TenantId, string TenantName, decimal CurrentBalance);