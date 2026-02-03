using Application.Security.Requirements;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Application.Security.Handlers;

    public class TenantScopeAccessHandler(
    ICurrentUserProvider currentUserProvider,
    IHttpContextAccessor httpContext,
    IConfiguration configuration) : AuthorizationHandler<TenantScopeAccessRequirement>
{
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IHttpContextAccessor _httpContext = httpContext;
    private readonly string _hostTenantId = configuration["Tenancy:Host"]
                                            ?? throw new Exception("Tenancy:Host not found");

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantScopeAccessRequirement requirement)
    {
        if (!_currentUserProvider.IsAuthenticated)
            return Task.CompletedTask;

        // SuperAdmin access
        if (_currentUserProvider.TenantId != null &&
            _currentUserProvider.TenantId == _hostTenantId &&
            context.User.IsInRole(RoleConstants.SuperAdmin)) // _currentUserProvider.IsInRole("SuperAdmin") ??
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // TODO: Config the role and permissions for the Tenant access.
        // if (_currentUserProvider.TenantId == tenantRouteValue)
        // {
        //     context.Succeed(requirement);
        //     return Task.CompletedTask;
        // }

        var tenantRouteValue = _httpContext.HttpContext?.GetRouteValue("tenantId")?.ToString();
        if ((string.IsNullOrEmpty(tenantRouteValue) &&
            _currentUserProvider.TenantId != null &&
            _currentUserProvider.IsInRole(RoleConstants.TenantOwner))
            ||
            (_currentUserProvider.TenantId == tenantRouteValue &&
            _currentUserProvider.IsInRole(RoleConstants.TenantOwner)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        // Tenant Owner
        // Security Fix: Ensure TenantOwner can only access their OWN tenant.
        if (_currentUserProvider.TenantId != null &&
            _currentUserProvider.IsInRole(RoleConstants.TenantOwner) &&
            _currentUserProvider.TenantId == tenantRouteValue)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}