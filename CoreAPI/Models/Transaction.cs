using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public record Transaction : ITenantEntity
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = null!;
    public string CustomerId { get; private set; } = null!;
    public string AccountTypeId { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string TransactionTypeId { get; set; } = null!;
    public string? Reason { get; private set; }
    public string? ReferenceId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    
    public TransactionType? TransactionType { get; set; }
    public Customer? Customer { get; set; }
    public string? PerformBy { get; init; }
    public User? Performer { get; set; }

    private Transaction()
    {
        var now = DateTimeOffset.UtcNow;
        OccurredAt = now;
        CreatedAt = now;
    }

private Transaction(
    string id,
    string tenantId,
    string customerId,
    string accountTypeId,
    decimal amount,
    string type,
    string? reason,
    string? referenceId = null,
    string? performBy = null,
    DateTimeOffset? occurredAt = null)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        AccountTypeId = accountTypeId;
        Amount = amount;
        TransactionTypeId = type;
        Reason = reason;
        ReferenceId = referenceId;
        PerformBy = performBy;
        var now = DateTimeOffset.UtcNow;
        OccurredAt = occurredAt ?? now;
        CreatedAt = now;
    }
    
    public static Transaction Create(
        string tenantId,
        string customerId,
        string accountTypeId,
        decimal amount,
        string type,
        string? reason,
        string? referenceId = null,
        string? performBy = null,
        DateTimeOffset? occurredAt = null)
        => new Transaction(Guid.NewGuid().ToString(), tenantId, customerId, accountTypeId, amount, type, reason, referenceId, performBy, occurredAt);
}