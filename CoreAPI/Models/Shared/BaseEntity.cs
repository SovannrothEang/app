namespace CoreAPI.Models.Shared;

public abstract class BaseEntity 
{
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; } = false;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; } = null;
    public DateTimeOffset? DeletedAt { get; private set; } = null;
    public string? PerformBy { get; private set; }

    protected void Modified()
    {
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }
    protected void Deleted()
    {
        this.IsDeleted = true;
        this.DeletedAt = DateTimeOffset.UtcNow;
    }
    public virtual void Activate()
    {
        if (this.IsActive)
            throw new InvalidOperationException("This entity is already active");
        this.IsActive = true;
        this.Modified();
    }
    public virtual void Deactivate()
    {
        if (!this.IsActive)
            throw new InvalidOperationException("This entity is already deactivated");
        this.IsActive = false;
        this.Modified();
    }

    public void AddPerformBy(string? userId)
    {
        this.PerformBy =  userId;
    }
}