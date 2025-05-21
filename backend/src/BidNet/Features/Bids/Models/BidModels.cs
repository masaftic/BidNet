namespace BidNet.Features.Bids.Models;

public class BidResponse
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public bool IsWinning { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlaceBidRequest
{
    public Guid AuctionId { get; set; }
    public decimal Amount { get; set; }
}

public class AuctionBidsResponse
{
    public Guid AuctionId { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal? CurrentPrice { get; set; }
    public BidResponse? WinningBid { get; set; }
    public IEnumerable<BidResponse> BidHistory { get; set; } = new List<BidResponse>();
}

public class UserBidsResponse
{
    public Guid UserId { get; set; }
    public IEnumerable<BidResponse> Bids { get; set; } = new List<BidResponse>();
}
