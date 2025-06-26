using Ardalis.GuardClauses;
using BidNet.Domain.Enums;

namespace BidNet.Domain.Entities;

public readonly record struct PaymentTransactionId(Guid Value)
{
    public static implicit operator Guid(PaymentTransactionId id) => id.Value;
    public static implicit operator PaymentTransactionId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class PaymentTransaction
{
    public PaymentTransactionId Id { get; init; }
    public AuctionId AuctionId { get; private set; }
    public Auction Auction { get; private set; } = null!;
    public UserId UserId { get; private set; }
    public User User { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private PaymentTransaction() { }

    public PaymentTransaction(AuctionId auctionId, UserId userId, decimal amount)
    {
        Guard.Against.Default(auctionId.Value, nameof(auctionId));
        Guard.Against.Default(userId.Value, nameof(userId));
        Guard.Against.NegativeOrZero(amount, nameof(amount));

        Id = Guid.NewGuid();
        AuctionId = auctionId;
        UserId = userId;
        Amount = amount;
    }

    public void UpdateStatus(PaymentStatus status)
    {
        Status = status;
    }
}
