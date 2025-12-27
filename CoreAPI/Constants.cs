namespace CoreAPI;

public static class Constants
{
    public const string RequireSuperAdminRole = "SuperAdminOnly";
    public const string RequireTenantOwner = "RequireTenantOwner";
    public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";
    public const string CanCreate = "CanCreate";
    public const string RequireTenantOwnerOrAdminRole = "RequireTenantOwnerOrAdmin";
    public const string TenantAccessPolicy = "TenantAccessPolicy";
    public const string PlatformRootPolicy = "PlatformRootPolicy";
}
