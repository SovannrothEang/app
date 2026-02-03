namespace Application;

public static class Constants
{
    // public const string RequireSuperAdminRole = "SuperAdminOnly";
    // public const string RequireTenantOwner = "RequireTenantOwner";
    public const string RequireAuthenticatedUser = "RequireAuthenticatedUser"; // Login User
    public const string PlatformRootAccessPolicy = "PlatformRootAccessPolicy"; // SuperAdmin
    public const string TenantCustomerAccessPolicy = "TenantCustomerAccessPolicy"; // SuperAdmin, Tenant, Customer
    public const string TenantScopeAccessPolicy = "TenantScopeAccessPolicy"; // SuperAdmin, Tenant
    public const string CustomerAccessPolicy = "CustomerAccessPolicy"; // SuperAdmin, Customer
}

public static class RoleConstants
{
    public const string TenantOwner = "TenantOwner";
    public const string SuperAdmin = "SuperAdmin";
    public const string Customer = "Customer";
    public const string User = "User";
}
