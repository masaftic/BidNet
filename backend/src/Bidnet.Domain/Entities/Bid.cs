using Ardalis.GuardClauses;

namespace Bidnet.Domain.Entities;

public readonly record struct BidId(Guid Value)
{
    public static implicit operator Guid(BidId id) => id.Value;
    public static implicit operator BidId(Guid id) => new(id);
}

public class Bid
{
    public BidId Id { get; set; }
    public AuctionId AuctionId { get; private set; }
    public UserId UserId { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsWinning { get; private set; } = false;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Bid() { }

    public Bid(AuctionId auctionId, UserId userId, decimal amount)
    {
        Guard.Against.Default(auctionId.Value, nameof(auctionId));
        Guard.Against.Default(userId.Value, nameof(userId));
        Guard.Against.NegativeOrZero(amount, nameof(amount));

        Id = Guid.NewGuid();
        AuctionId = auctionId;
        UserId = userId;
        Amount = amount;
    }
    
    public void UpdateWinningStatus(bool isWinning)
    {
        IsWinning = isWinning;
    }
}
