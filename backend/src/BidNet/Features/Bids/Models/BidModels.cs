namespace BidNet.Features.Bids.Models;

public class BidDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
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
    public BidDto? WinningBid { get; set; }
    public IEnumerable<BidDto> BidHistory { get; set; } = [];
}

public class UserBidsResponse
{
    public Guid UserId { get; set; }
    public IEnumerable<BidDto> Bids { get; set; } = [];
}
