using CoreAPI.Models.Shared;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Models;

public sealed class User : IdentityUser<string>, IEntity
{
    public string? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; } = false;
    public DateTime CreatedAt { get; } = DateTime.UtcNow.AddHours(7);
    public DateTime? UpdatedAt { get; private set; } = null;
    
    // private readonly List<TenantUser> _tenantUsers = [];
    // public IReadOnlyCollection<TenantUser> TenantUsers => _tenantUsers;

    private User()
    {
        Id = Guid.NewGuid().ToString();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public User(string email, string userName)
    {
        Id = Guid.NewGuid().ToString();
        SecurityStamp = Guid.NewGuid().ToString();
        UserName = userName;
        Email = email;
    }
}
