using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CoreAPI.Requirements.Handlers;

public class PlatformRootAccessHandler(ICurrentUserProvider currentUserProvider, IConfiguration configuration) : AuthorizationHandler<PlatformRootAccessRequirement>
{
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly string _tenantHost = configuration["Tenancy:Host"]
                                          ?? throw new Exception("Tenant host not found");

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PlatformRootAccessRequirement requirement)
    {
        if (!_currentUserProvider.IsAuthenticated)
            return Task.CompletedTask;
        
        if (_currentUserProvider.TenantId == _tenantHost && _currentUserProvider.IsInRole("SuperAdmin"))
            context.Succeed(requirement);
        
        return Task.CompletedTask;
    }
}