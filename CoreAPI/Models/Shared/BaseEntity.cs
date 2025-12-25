namespace CoreAPI.Models.Shared;

public abstract class BaseEntity 
{
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; } = false;
    public DateTime CreatedAt { get; } = DateTime.UtcNow.AddHours(7);
    public DateTime? UpdatedAt { get; private set; } = null;
    public void Modified()
    {
        this.UpdatedAt = DateTime.UtcNow.AddHours(7);
    }
    public virtual void Activate()
    {
        if (this.IsActive)
            throw new InvalidOperationException("This entity is already active");
        this.IsActive = true;
    }
    public virtual void Deactivate()
    {
        if (!this.IsActive)
            throw new InvalidOperationException("This entity is already deactivated");
        this.IsActive = false;
    }

}