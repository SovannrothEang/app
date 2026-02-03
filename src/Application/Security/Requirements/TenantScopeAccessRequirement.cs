using Microsoft.AspNetCore.Authorization;

namespace Application.Security.Requirements;

public class TenantScopeAccessRequirement() : IAuthorizationRequirement { }