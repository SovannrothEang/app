using CoreAPI.Models.Enums;
using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public record Transaction : ITenantEntity
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = null!;
    public string CustomerId { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Reason { get; private set; }
    public string? ReferenceId { get; private set; }
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;
    // Todo: PerformBy which user

    public Account? LoyaltyAccount { get; init; }

    private Transaction() { }

    public Transaction(string id, string tenantId, string customerId, decimal amount, TransactionType type, string? reason, string? referenceId = null)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        Amount = amount;
        Type = type;
        Reason = reason;
        ReferenceId = referenceId;
    }
    
    public static Transaction Create(
        string tenantId,
        string customerId,
        int amount,
        TransactionType type,
        string? reason,
        string? referenceId = null)
        => new Transaction(Guid.NewGuid().ToString(), tenantId, customerId, amount, type, reason, referenceId);
}