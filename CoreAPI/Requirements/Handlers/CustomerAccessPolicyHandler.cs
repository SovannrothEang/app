using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CoreAPI.Requirements.Handlers;

public class CustomerAccessPolicyHandler(
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserProvider currentUserProvider) : AuthorizationHandler<CustomerAccessPolicyRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly string _tenantHost = configuration["Tenancy:Host"]
                                          ?? throw new Exception("Tenant host not found");

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomerAccessPolicyRequirement requirement)
    {
        if (!_currentUserProvider.IsAuthenticated)
            return Task.CompletedTask;
        
        // Checking Customer
        var customerRouteValue = _httpContextAccessor.HttpContext?.GetRouteValue("customerId")?.ToString();
        if ((string.IsNullOrEmpty(customerRouteValue) &&
            _currentUserProvider.CustomerId != null &&
            _currentUserProvider.IsInRole(RoleConstants.Customer))
            ||
            (_currentUserProvider.CustomerId == customerRouteValue &&
             _currentUserProvider.IsInRole(RoleConstants.Customer)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // SuperAdmin
        if (_currentUserProvider.TenantId == _tenantHost &&
            _currentUserProvider.IsInRole(RoleConstants.SuperAdmin))
            context.Succeed(requirement);
        
        return Task.CompletedTask;
    }
}
