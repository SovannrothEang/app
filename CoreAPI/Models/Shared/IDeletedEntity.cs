namespace CoreAPI.Models.Shared;

public interface IDeletedEntity
{
    public bool IsDeleted { get; set; } 
    public DateTimeOffset? DeletedAt { get; set; }
    void Deleted();
}
