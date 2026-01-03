using CoreAPI.Data;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CoreAPI.Requirements.Handlers;

public class TenantCustomerAccessHandler(
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserProvider currentUserProvider) : AuthorizationHandler<TenantCustomerAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly string _tenantHost = configuration["Tenancy:Host"]
                                          ?? throw new Exception("Tenant host not found");

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        TenantCustomerAccessRequirement requirement)
    {
        if (!_currentUserProvider.IsAuthenticated)
            return Task.CompletedTask;

        // Customer, access by checking their ID
        // I still think that there will be a leak, TODO: make sure everything is fine
        var customerRouteId = _httpContextAccessor.HttpContext?.GetRouteValue("customerId")?.ToString();
        if (string.IsNullOrEmpty(customerRouteId) &&
            _currentUserProvider.CustomerId != null &&
            _currentUserProvider.IsInRole(RoleConstants.Customer))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        if (_currentUserProvider.IsInRole(RoleConstants.Customer) && _currentUserProvider.CustomerId == customerRouteId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Tenant, access by checking their ID
        // Already has RLS, by making sure their scope of use is being shown, no extra
        var tenantRouteValue = _httpContextAccessor.HttpContext?.GetRouteValue("tenantId")?.ToString();
        if (string.IsNullOrEmpty(tenantRouteValue) &&
            _currentUserProvider.CustomerId != null &&
            _currentUserProvider.IsInRole(RoleConstants.Customer))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        if (_currentUserProvider.TenantId == tenantRouteValue && _currentUserProvider.IsInRole(RoleConstants.TenantOwner)) // TODO: Add role checking
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // SuperAdmin
        // Bypass the authorize policy, still need to access through .IgnoreQueryFilters()
        if (_currentUserProvider.TenantId == _tenantHost && _currentUserProvider.IsInRole(RoleConstants.SuperAdmin))
            context.Succeed(requirement);
        
        return Task.CompletedTask;
    }
}