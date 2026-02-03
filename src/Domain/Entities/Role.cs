using Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public sealed class Role : IdentityRole<string>, ITenantEntity, IAuditEntity, IDeletedEntity
{
    public string TenantId { get; set; } = null!;

    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? PerformBy { get; set; }
    
    public User? Performer { get; set; }
    public ICollection<UserRole>? UserRoles { get; set; } = [];
    public Tenant? Tenant { get; set; }

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
