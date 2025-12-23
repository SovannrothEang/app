using CoreAPI.Models.Enums;
using CoreAPI.Models.Shared;

namespace CoreAPI.Models;

public sealed class LoyaltyAccount : BaseEntity
{
    public string TenantId { get; private set; } = null!;
    public string CustomerId { get; private set; } = null!;
    public int Balance { get; private set; }
    public TierLevel Tier { get; private set; } = TierLevel.Bronze;
    
    private readonly List<PointTransaction> _pointTransactions = [];
    public IReadOnlyCollection<PointTransaction> PointTransactions => _pointTransactions.AsReadOnly();

    public LoyaltyAccount() { }

    public LoyaltyAccount(string tenantId, string customerId)
    {
        TenantId = tenantId;
        CustomerId = customerId;
    }
   
    public (int balanace, PointTransaction transaction)
        EarnPoint(
            int amount,
            string? reason,
            string? referenceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        var transaction = ApplyTransaction(amount, TransactionType.Earn, reason, referenceId);
        return (this.Balance, transaction);
    }
    
    public (int balance, PointTransaction transaction)
        Redemption (int amount, string? reason)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (amount > Balance)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Amount cannot be greater than {nameof(Balance)}");
        var transaction = ApplyTransaction(-amount, TransactionType.Redeem, reason, null);
        return (this.Balance, transaction);
    }

    public (int balance, PointTransaction transaction)
        Adjustment (int amount, string? reason, string? referenceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        var transaction = ApplyTransaction(amount, TransactionType.Adjustment, reason, referenceId);
        return (this.Balance, transaction);
    }

    private PointTransaction ApplyTransaction(
        int amount,
        TransactionType type,
        string? reason,
        string? referenceId)
    {
        this.Balance += amount;
        Tier = this.Balance switch
        {
            >= 1000 => TierLevel.Gold,
            >= 500 => TierLevel.Silver,
            _ => TierLevel.Bronze
        };

        var transaction = PointTransaction.Create(
            TenantId,
            CustomerId,
            amount,
            type,
            reason,
            referenceId);
        _pointTransactions.Add(transaction);

        return transaction;
    }
}