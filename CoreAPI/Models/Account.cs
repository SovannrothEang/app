using CoreAPI.Models.Enums;
using CoreAPI.Models.Shared;
using System.ComponentModel.DataAnnotations;
using CoreAPI.Exceptions;

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
   
    public (decimal balance, Transaction transaction) ProcessTransaction(
        decimal signedAmount,
        string transactionTypeId,
        string? reason,
        string? referenceId,
        string? performBy)
    {
        // Safety Force: If negative (Redemption), ensure balance is sufficient
        if (signedAmount < 0 && (Balance + signedAmount) < 0)
        {
            throw new InsufficientBalanceException(Balance, Math.Abs(signedAmount));
        }
        
        var transaction = ApplyTransaction(signedAmount, transactionTypeId, reason, referenceId, performBy);
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