using Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public sealed class User : IdentityUser<string>, ITenantEntity, IAuditEntity, IDeletedEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    
    public string AuthProvider { get; set; } = "Local";
    public string? ProviderKey { get; set; }
    
    public string TenantId { get; set; } = null!;
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; } = null;
    public DateTimeOffset? DeletedAt { get; set; } = null;
    public string? PerformBy { get; set; }
    
    public User? Performer { get; set; }
    public Tenant? Tenant { get; set; }

    public ICollection<UserRole>? UserRoles { get; set; } = [];

    private User()
    {
        Id = Guid.NewGuid().ToString();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public User(string id, string email, string userName, string tenantId)
    {
        Id = id;
        SecurityStamp = Guid.NewGuid().ToString();
        UserName = userName;
        Email = email;
        TenantId = tenantId;
        if (string.IsNullOrEmpty(TenantId)) throw new ArgumentNullException(nameof(tenantId));
    }

    public void Modified()
    {
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deleted()
    {
        this.IsDeleted = true;
        this.DeletedAt = DateTimeOffset.UtcNow;
    }
    public void Activate()
    {
        if (this.IsActive)
            throw new InvalidOperationException("This entity is already active");
        this.IsActive = true;
        Modified();
    }
    public void Deactivate()
    {
        if (!this.IsActive)
            throw new InvalidOperationException("This entity is already deactivated");
        this.IsActive = false;
        Modified();
    }
}
