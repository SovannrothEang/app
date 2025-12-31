using CoreAPI.Models.Shared;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Models;

public sealed class UserRole : IdentityUserRole<string>, ITenantEntity
{
    public string TenantId { get; set; } = null!;

    
    public User? User { get; set; }
    public Role? Role { get; set; }
    
    public UserRole() { }
    public  UserRole(string userId, string roleId, string tenantId)
    {
        UserId = userId;
        RoleId = roleId;
        TenantId = tenantId;
    }
}