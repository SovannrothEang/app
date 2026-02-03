namespace Application.Services;

public interface ICurrentUserProvider
{
    string? UserId { get; }
    string? Email { get; }
    string? TenantId { get; }
    string? CustomerId { get; }
    public void SetTenantId(string tenantId);
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    IReadOnlyList<string> Roles { get; }
}
