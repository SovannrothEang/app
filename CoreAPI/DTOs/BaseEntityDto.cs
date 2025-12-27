namespace CoreAPI.DTOs;

public abstract class BaseEntityDto
{
    public string Id { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = default!;
    public DateTimeOffset? UpdatedAt { get; set; } = null;
}
