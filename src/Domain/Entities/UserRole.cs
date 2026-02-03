using Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public sealed class UserRole : IdentityUserRole<string>, ITenantEntity
{
    public string TenantId { get; set; } = null!;

    
    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("UserId, TenantId")]
    public User? User { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("RoleId, TenantId")]
    public Role? Role { get; set; }
    
    public UserRole() { }
    public  UserRole(string userId, string roleId, string tenantId)
    {
        UserId = userId;
        RoleId = roleId;
        TenantId = tenantId;
    }
}