using System.Diagnostics;
using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public sealed class TransactionType : BaseEntity, ITenantEntity
{
    public string Id { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Url { get; private set; } = null!;
    public string TenantId { get; set; } = null!;

    public int Multiplier { get; private set; } // -1 for redeem, reduction, 1 for bonus, gift, ...
    public bool AllowNegative { get; private set; } = false;
    
    public User? Performer { get; set; }
    public Tenant? Tenant { get; set; }
    public IReadOnlyCollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    
    public TransactionType() {}

    public TransactionType(string id,string slug, string name, string? description, int multiplier, bool allowNegative, string tenantId)
    {
        Id = id;
        Slug = slug.ToLower();
        Name = name;
        Description = description;
        Url = "/api/tenants/{tenantId}/customers/{customerId}/accountTypes/{accountTypeId}" + slug.ToLower();
        Multiplier = multiplier switch
        {
            -1 => -1,
            1 => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "Value can only be 1 or -1.")
        };
        AllowNegative = allowNegative;
        TenantId = tenantId;
    }
}