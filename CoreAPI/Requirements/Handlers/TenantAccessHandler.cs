using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CoreAPI.Requirements.Handlers;

public class TenantAccessHandler(
    ICurrentUserProvider currentUserProvider,
    IHttpContextAccessor httpContext,
    IConfiguration configuration) : AuthorizationHandler<TenantAccessRequirement, string>
{
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IHttpContextAccessor _httpContext = httpContext;
    private readonly string _hostTenantId = configuration["Tenancy:Host"]
                                            ?? throw new Exception("Tenancy:Host not found");

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAccessRequirement requirement,
        string tenantId)
    {
        if (!_currentUserProvider.IsAuthenticated)
            return Task.CompletedTask;
        
        // SuperAdmin access
        if (_currentUserProvider.TenantId == _hostTenantId &&
            !context.User.IsInRole("SuperAdmin")) // _currentUserProvider.IsInRole("SuperAdmin") ??
        {
            context.Succeed(requirement);
        }
        
        var tenantRouteValue = _httpContext.HttpContext?.GetRouteValue("tenantId")?.ToString();
        // Tenant Owner Access
        // TODO: Need to check the Tenant's role, and permissions
        if (_currentUserProvider.TenantId == tenantRouteValue)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}