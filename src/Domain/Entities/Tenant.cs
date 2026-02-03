using Domain.Enums;
using Domain.Shared;

namespace Domain.Entities;

public sealed class Tenant : BaseEntity
{
    public string Id { get; private set; } 
    public string Name { get; private set; } = null!;
    public TenantStatus Status { get; private set; } =  TenantStatus.Active;
    public AccountSetting? Setting { get; private set; }
    public string Slug { get; set; } = null!;

    
    private readonly List<User> _users = [];
    public IReadOnlyCollection<User> Users => _users;
    private readonly List<Role> _roles = [];
    public IReadOnlyCollection<Role> Roles => _roles;
    private readonly List<Account> _accounts = [];
    public IReadOnlyCollection<Account> Accounts => _accounts;
    private readonly List<AccountType> _accountTypes = [];
    public IReadOnlyCollection<AccountType> AccountTypes => _accountTypes;
    public User? PerformByUser { get; set; }

    public Tenant()
    {
        Id = Guid.NewGuid().ToString();
    }
    
    public Tenant(string id, string name, string slug, AccountSetting? setting = null, string? performBy = null)
    {
        Id = id;
        Name = name;
        Slug =  slug;
        Setting = setting;
        AddPerformBy(performBy);
    }

    public override void Deactivate()
    {
        Status = TenantStatus.Inactive;
        Modified();
    }

    public override void Activate()
    {
        Status = TenantStatus.Active;
        Modified();
    }

    public void UpdateSetting(AccountSetting setting)
    {
        Setting = setting;
        Modified();
    }
}
