namespace CoreAPI;

public static class Constants
{
    public const string RequireSuperAdminRole = "SuperAdminOnly";
    public const string RequireTenantOwner = "RequireTenantOwner";
    public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";
    public const string TenantScopeAccessPolicy = "TenantScopeAccessPolicy"; // SuperAdmin, Tenant
    public const string PlatformRootPolicy = "PlatformRootPolicy"; // SuperAdmin
    public const string TransactionAccessPolicy = "TransactionAccessPolicy "; // SuperAdmin, Tenant, Customer
}
