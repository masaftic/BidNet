namespace BidNet.Features.Bids.Models;

public class AuctionUpdatedDto
{
    public Guid AuctionId { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime LastBidTime { get; set; }
    public Guid LastBidUserId { get; set; }
    public string LastBidUserName { get; set; } = string.Empty;
    public int BidCount { get; set; }
}
