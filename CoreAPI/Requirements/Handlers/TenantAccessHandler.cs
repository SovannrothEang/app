using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CoreAPI.Requirements.Handlers;

public class TenantAccessHandler(ICurrentUserProvider currentUserProvider, IUnitOfWork unitOfWork) : AuthorizationHandler<TenantAccessRequirement, string>
{
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAccessRequirement requirement,
        string tenantId)
    {
        if (!_currentUserProvider.IsAuthenticated)
            return;
        
        // if (currentUserTenantId is null &&
        //     !context.User.IsInRole("SuperAdmin"))
        // {
        //     return Task.CompletedTask;
        // }
        
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant != null && tenant.Id == _currentUserProvider.TenantId ||
            _currentUserProvider.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
        }

    }
}