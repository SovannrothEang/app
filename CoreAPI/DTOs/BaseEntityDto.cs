namespace CoreAPI.DTOs;

public abstract class BaseEntityDto
{
    public string Id { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = default!;
    public DateTime? UpdatedAt { get; set; } = null;
}
