using Microsoft.AspNetCore.Authorization;

namespace CoreAPI.Requirements;

public class TenantScopeAccessRequirement() : IAuthorizationRequirement { }