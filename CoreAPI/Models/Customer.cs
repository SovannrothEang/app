using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public sealed class Customer : BaseEntity
{
    public string Id { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;

    private readonly List<LoyaltyAccount> _accounts = [];
    public IReadOnlyCollection<LoyaltyAccount> LoyaltyAccounts => _accounts;
    
    private Customer() { }

    public Customer(string id, string name, string email, string phoneNumber)
    {
        if (name.Length is 0 or > 150)
            throw new ArgumentException($"The name {name} is invalid.");
        
        if (phoneNumber.Length is 0 or > 150)
            throw new ArgumentException($"The phone number {phoneNumber} is invalid.");

        Id = id;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
    }
    
    public LoyaltyAccount CreateLoyaltyAccount(string tenantId)
    {
        if (_accounts.Any(e => Equals(e.TenantId, tenantId)))
            throw new ArgumentException($"The tenant {tenantId} is already created.");
        var account = new LoyaltyAccount(tenantId, this.Id);
        _accounts.Add(account);
        return account;
    }
}