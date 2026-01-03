using CoreAPI.Models.Enums;
using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public record Transaction : ITenantEntity
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = null!;
    public string CustomerId { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; } // TODO: Create a new model for TransactionType in tenant's scope
    public string? Reason { get; private set; }
    public string? ReferenceId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    
    public string? PerformBy { get; init; }
    public User? PerformByUser { get; set; }

    private Transaction()
    {
        var now = DateTimeOffset.UtcNow;
        OccurredAt = now;
        CreatedAt = now;
    }

public Transaction(
    string id,
    string tenantId,
    string customerId,
    decimal amount,
    TransactionType type,
    string? reason,
    string? referenceId = null,
    DateTimeOffset? occurredAt = null)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        Amount = amount;
        Type = type;
        Reason = reason;
        ReferenceId = referenceId;
        var now = DateTimeOffset.UtcNow;
        // TODO: recheck this logic, for ensuring if the transaction is correct.
        OccurredAt = occurredAt ?? now;
        CreatedAt = now;
    }
    
    public static Transaction Create(
        string tenantId,
        string customerId,
        decimal amount,
        TransactionType type,
        string? reason,
        string? referenceId = null,
        DateTimeOffset? occurredAt = null)
        => new Transaction(Guid.NewGuid().ToString(), tenantId, customerId, amount, type, reason, referenceId, occurredAt);
}