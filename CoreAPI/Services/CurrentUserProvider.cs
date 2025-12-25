using System.Security.Claims;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    public string? UserId => _httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? Email => _httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                           ?? _httpContext?.User?.Identity?.Name;
    public string? TenantId => _httpContext?.User?.FindFirst("tenant_id")?.Value;
    public bool IsAuthenticated => _httpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        _httpContext?.User?.IsInRole(role) ?? false;

    public IReadOnlyList<string> Roles =>
        _httpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];
}