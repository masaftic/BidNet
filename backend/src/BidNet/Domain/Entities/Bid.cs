using Ardalis.GuardClauses;

namespace BidNet.Domain.Entities;

public readonly record struct BidId(Guid Value)
{
    public static implicit operator Guid(BidId id) => id.Value;
    public static implicit operator BidId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class Bid
{
    public BidId Id { get; init; }
    public AuctionId AuctionId { get; private set; }
    public Auction Auction { get; private set; } = null!;
    public UserId UserId { get; private set; }
    public User User { get; private set; } = null!;
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
