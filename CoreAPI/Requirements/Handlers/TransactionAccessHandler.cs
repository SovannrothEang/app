using CoreAPI.Data;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CoreAPI.Requirements.Handlers;

public class TransactionAccessHandler(
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserProvider currentUserProvider) : AuthorizationHandler<TransactionAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly string _tenantHost = configuration["Tenancy:Host"]
                                          ?? throw new Exception("Tenant host not found");

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        TransactionAccessRequirement requirement)
    {
        if (!_currentUserProvider.IsAuthenticated)
            return Task.CompletedTask;

        var customerRouteId = _httpContextAccessor.HttpContext?.GetRouteValue("customerId")?.ToString();
        if (_currentUserProvider.IsInRole("Customer") && _currentUserProvider.CustomerId == customerRouteId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // IDK if we should check the relationship between tenant and customer in this or not
        var tenantRouteId = _httpContextAccessor.HttpContext?.GetRouteValue("tenantId")?.ToString();
        if (_currentUserProvider.TenantId == tenantRouteId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (_currentUserProvider.TenantId == _tenantHost && _currentUserProvider.IsInRole("SuperAdmin"))
            context.Succeed(requirement);
        
        return Task.CompletedTask;
    }
}