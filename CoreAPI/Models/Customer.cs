using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public sealed class Customer : BaseEntity
{
    public string Id { get; private set; } = null!;
    public string UserId { get; set; } = null!;
    public User? User { get; set; }

    private readonly List<Account> _accounts = [];
    public IReadOnlyCollection<Account> Accounts => _accounts;
    
    public User? Performer { get; set; }
    
    private Customer() { }

    public Customer(string id, string userId, string? performBy)
    {
        Id = id;
        UserId = userId;
        AddPerformBy(performBy);
    }
    
    public Account CreateLoyaltyAccount(string tenantId)
    {
        if (_accounts.Any(e => Equals(e.TenantId, tenantId)))
            throw new ArgumentException($"The tenant {tenantId} is already created.");
        var account = new Account(tenantId, this.Id);
        _accounts.Add(account);
        return account;
    }
}