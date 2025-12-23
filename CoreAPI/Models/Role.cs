using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Models;

public sealed class Role : IdentityRole<string>
{
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;

    public Role()
    {
        Id = Guid.NewGuid().ToString();
    }
}
