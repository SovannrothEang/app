using CoreAPI.Models.Enums;
using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public sealed class Tenant : BaseEntity
{
    public string Id { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public TenantStatus Status { get; private set; } =  TenantStatus.Active;
    public LoyaltyProgramSetting Setting { get; private set; } = null!;

    public User? User { get; set; }
    
    // private readonly List<TenantUser> _tenantUsers = [];
    // public IReadOnlyCollection<TenantUser> TenantUsers => _tenantUsers;

    public Tenant() { }
    
    public Tenant(string id, string name, LoyaltyProgramSetting setting)
    {
        Name = name;
        Status = TenantStatus.Active;
        Setting = setting;
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

    public void UpdateSetting(LoyaltyProgramSetting setting)
    {
        Setting = setting;
        Modified();
    }
}