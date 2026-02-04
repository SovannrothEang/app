using Domain.Exceptions;
using Domain.Shared;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public sealed class Account : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = null!;
    public string CustomerId { get; set; } = null!;
    public string AccountTypeId { get; set; } = null!;
    public decimal Balance { get; private set; }
    
    private readonly List<Transaction> _transactions = [];
    public AccountType? AccountType { get; set; }
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public User? Performer { get; set; }
    // TODO: take a look at this
    [JsonIgnore]
    public Customer? Customer { get; set; }
    public Tenant? Tenant { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public Account() { }

    public Account(string tenantId, string customerId, string accountTypeId)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        AccountTypeId = accountTypeId;
    }
   
    public (decimal balance, Transaction transaction) ProcessTransaction(
        decimal signedAmount,
        string transactionTypeId,
        string? reason,
        string? referenceId,
        string? idempotencyKey,
        string? performBy,
        DateTimeOffset? occurredAt)
    {
        // Validate before update - check for negative balance
        if (Balance + signedAmount < 0)
        {
            throw new InsufficientBalanceException(Balance, Math.Abs(signedAmount));
        }
        
        var transaction = ApplyTransaction(signedAmount, transactionTypeId, reason, referenceId, idempotencyKey, performBy, occurredAt);
        return (this.Balance, transaction);
    }

    private Transaction ApplyTransaction(
        decimal amount,
        string type,
        string? reason,
        string? referenceId,
        string? idempotencyKey,
        string? performBy,
        DateTimeOffset? occurredAt)
    {
        this.Balance += amount;
        if (this.Balance < 0)
            throw new ArgumentOutOfRangeException(nameof(Balance), $"Balance cannot be negative");

        var transaction = Transaction.Create(
            TenantId,
            CustomerId,
            AccountTypeId,
            amount,
            type,
            reason,
            referenceId,
            idempotencyKey,
            performBy,
            occurredAt);
        _transactions.Add(transaction);

        return transaction;
    }
}