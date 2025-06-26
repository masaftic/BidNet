using BidNet.Domain.Enums;

namespace BidNet.Domain.Entities;

public readonly record struct WalletTransactionId(Guid Value)
{
    public static WalletTransactionId New() => new(Guid.NewGuid());

    public static implicit operator Guid(WalletTransactionId id) => id.Value;
    public static implicit operator WalletTransactionId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class WalletTransaction
{
    public WalletTransactionId Id { get; private set; }
    public WalletId WalletId { get; private set; }
    public UserId UserId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime Timestamp { get; private set; }
    public UserId? RecipientId { get; private set; }

    private WalletTransaction()
    {
        Description = string.Empty;
    }

    public WalletTransaction(
        WalletId walletId,
        UserId userId,
        TransactionType type,
        decimal amount,
        string description,
        DateTime timestamp,
        UserId? recipientId = null)
    {
        Id = WalletTransactionId.New();
        WalletId = walletId;
        UserId = userId;
        Type = type;
        Amount = amount;
        Description = description;
        Timestamp = timestamp;
        RecipientId = recipientId;
    }
}
