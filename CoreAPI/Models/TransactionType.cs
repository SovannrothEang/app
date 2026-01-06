using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public sealed class TransactionType : BaseEntity, ITenantEntity
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    
    public User? Performer { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    
    public TransactionType() {}

    public TransactionType(string id, string name, string tenantId, string performBy)
    {
        Id = id;
        Name = name;
        TenantId = tenantId;
        AddPerformBy(performBy);
    }
}