using Ardalis.GuardClauses;

namespace BidNet.Domain.Entities;

public readonly record struct AuctionImageId(Guid Value)
{
    public static implicit operator Guid(AuctionImageId id) => id.Value;
    public static implicit operator AuctionImageId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class AuctionImage
{
    public AuctionImageId Id { get; private set; }
    public AuctionId AuctionId { get; private set; }
    public Auction Auction { get; private set; } = null!;
    public string ImageUrl { get; private set; } = null!;
    public bool IsPrimary { get; private set; }
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

    private AuctionImage() { }

    public AuctionImage(AuctionId auctionId, string imageUrl, bool isPrimary = false)
    {
        Guard.Against.NullOrEmpty(imageUrl, nameof(imageUrl));

        Id = Guid.NewGuid();
        AuctionId = auctionId;
        ImageUrl = imageUrl;
        IsPrimary = isPrimary;
    }

    public void SetAsPrimary(bool isPrimary = true)
    {
        IsPrimary = isPrimary;
    }
}
