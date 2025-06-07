using BidNet.Domain.Entities;
using BidNet.Features.Bids.Models;

namespace BidNet.Features.Bids.Mapping;

public static class BiddingMappingExtensions
{
    public static IQueryable<BidDto> ToBidDto(this IQueryable<Bid> query)
    {
        return query.Select(b => new BidDto
        {
            Id = b.Id,
            AuctionId = b.AuctionId,
            UserId = b.UserId,
            UserName = b.User.Username,
            Amount = b.Amount,
            IsWinning = b.IsWinning,
            CreatedAt = b.CreatedAt
        });
    }

    public static BidDto ToBidDto(this Bid bid)
    {
        return new BidDto
        {
            Id = bid.Id,
            AuctionId = bid.AuctionId,
            UserId = bid.UserId,
            UserName = bid.User.Username,
            Amount = bid.Amount,
            IsWinning = bid.IsWinning,
            CreatedAt = bid.CreatedAt
        };
    }
}
