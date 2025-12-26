using System.Security.Claims;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CoreAPI.Requirements.Handlers;

public class PlatformAccessHandler(IConfiguration configuration, ICurrentUserProvider userProvider) : AuthorizationHandler<PlatformAccessRequirement>
{
    private readonly ICurrentUserProvider _userProvider = userProvider;
    private readonly string _hostTenantId = configuration["Tenancy:Host"]
                                            ?? throw new Exception("Tenancy:Host not found");

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PlatformAccessRequirement requirement)
    {
        var userTenant = _userProvider.TenantId;

        if (userTenant == _hostTenantId && _userProvider.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}