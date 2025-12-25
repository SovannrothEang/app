using CoreAPI.Models.Enums;
using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public record PointTransaction : ITenantEntity
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string TenantId { get; private set; } = null!;
    public string CustomerId { get; private set; } = null!;
    public int Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Reason { get; private set; }
    public string? ReferenceId { get; private set; }
    public DateTime OccurredOn { get; private set; } = DateTime.UtcNow;

    private PointTransaction() { }

    public PointTransaction(string id, string tenantId, string customerId, int amount, TransactionType type, string? reason, string? referenceId = null)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        Amount = amount;
        Type = type;
        Reason = reason;
        ReferenceId = referenceId;
    }
    
    public static PointTransaction Create(string tenantId, string customerId, int amount, TransactionType type, string? reason, string? referenceId = null)
        => new PointTransaction(Guid.NewGuid().ToString(), tenantId, customerId, amount, type, reason, referenceId);
}