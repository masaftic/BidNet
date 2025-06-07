using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Models;

namespace BidNet.Features.Auctions.Mapping;

public static class AuctionMappingExtensions
{
    public static IQueryable<AuctionDto> ToAuctionDto(this IQueryable<Auction> auctions)
    {
        return auctions.Select(auction => new AuctionDto(
            auction.Id,
            auction.Title,
            auction.Description,
            auction.StartDate,
            auction.EndDate,
            auction.StartingPrice,
            auction.CreatedBy));
    }
}