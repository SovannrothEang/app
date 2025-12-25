using CoreAPI.Models.Shared;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Models;

public sealed class Role : IdentityRole<string>, ITenantEntity
{
    public string TenantId { get; private set; } = null!;
    
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;

    public Role()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Role(string id, string name, string tenantId)
    {
        Id = id;
        Name = name;
        TenantId = tenantId;
    }
}
