using CoreAPI.Models.Enums;
using CoreAPI.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace CoreAPI.Models;

public sealed class Account : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = null!;
    public string CustomerId { get; private set; } = null!;
    public decimal Balance { get; private set; }
    
    private readonly List<Transaction> _transactions = [];
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public User? PerformByUser { get; set; }
    public Customer? Customer { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public Account() { }

    public Account(string tenantId, string customerId)
    {
        TenantId = tenantId;
        CustomerId = customerId;
    }
   
    public (decimal balanace, Transaction transaction) EarnPoint(
            decimal amount,
            string? reason,
            string transactionTypeId,
            string? referenceId,
            string? performBy)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        var transaction = ApplyTransaction(amount, transactionTypeId, reason, referenceId, performBy);
        return (this.Balance, transaction);
    }
    
    public (decimal balance, Transaction transaction) Redemption (
        decimal amount,
        string? reason,
        string transactionTypeId,
        string? performBy)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (amount > Balance)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Amount cannot be greater than {nameof(Balance)}");
        var transaction = ApplyTransaction(-amount, transactionTypeId, reason, null, performBy);
        return (this.Balance, transaction);
    }

    public (decimal balance, Transaction transaction) Adjustment (
        decimal amount,
        string? reason,
        string? referenceId,
        string transactionTypeId,
        string? performBy)
    {
        // Adjustment can be negative number
        // ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        var transaction = ApplyTransaction(amount, transactionTypeId, reason, referenceId, performBy);
        return (this.Balance, transaction);
    }

    private Transaction ApplyTransaction(
        decimal amount,
        string type,
        string? reason,
        string? referenceId,
        string? performBy)
    {
        this.Balance += amount;
        if (this.Balance < 0)
            throw new ArgumentOutOfRangeException(nameof(Balance), $"Balance cannot be negative");

        var transaction = Transaction.Create(
            TenantId,
            CustomerId,
            amount,
            type,
            reason,
            referenceId,
            performBy);
        _transactions.Add(transaction);

        return transaction;
    }
}