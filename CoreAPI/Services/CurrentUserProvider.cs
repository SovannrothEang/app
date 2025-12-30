using System.Security.Claims;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                           ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public string? TenantId { get; private set; } = httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
    public string? CustomerId { get; } = httpContextAccessor.HttpContext?.User?.FindFirst("customer_id")?.Value;
    public void SetTenantId(string tenantId) => TenantId = tenantId;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

    public IReadOnlyList<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];
}