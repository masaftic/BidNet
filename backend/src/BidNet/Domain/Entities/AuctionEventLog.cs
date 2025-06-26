using Ardalis.GuardClauses;
using BidNet.Domain.Enums;

namespace BidNet.Domain.Entities;

public readonly record struct AuctionEventLogId(Guid Value)
{
    public static implicit operator Guid(AuctionEventLogId id) => id.Value;
    public static implicit operator AuctionEventLogId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class AuctionEventLog
{
    public AuctionEventLogId Id { get; init; }
    public AuctionId AuctionId { get; private set; }
    public Auction Auction { get; private set; } = null!;
    public AuctionEventType EventType { get; private set; }
    public string Details { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private AuctionEventLog() { }

    public AuctionEventLog(AuctionId auctionId, AuctionEventType eventType, string details)
    {
        Guard.Against.Default(auctionId.Value, nameof(auctionId));
        Guard.Against.NullOrEmpty(details, nameof(details));

        Id = Guid.NewGuid();
        AuctionId = auctionId;
        EventType = eventType;
        Details = details;
    }
}
