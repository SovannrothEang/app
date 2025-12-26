namespace CoreAPI.Services.Interfaces;

public interface ICurrentUserProvider
{
    string? UserId { get; }
    string? Email { get; }
    string? TenantId { get; }
    public void SetTenantId(string tenantId);
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    IReadOnlyList<string> Roles { get; }
}
