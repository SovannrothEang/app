namespace Domain.Shared;

public interface IAuditEntity
{
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
