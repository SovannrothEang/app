using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public class AccountType: BaseEntity, ITenantEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string TenantId { get; set; } =  string.Empty;
    
    public User? Performer { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<Account> Accounts { get; set; } = [];

    public AccountType() { }
    
    public AccountType(string id, string name, string tenantId, string? performBy)
    {
        Id = id;
        Name = name;
        TenantId = tenantId;
        AddPerformBy(performBy);
    }
}