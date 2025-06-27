using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BidNet.Features.Auctions.Mapping;

public static class AuctionMappingExtensions
{
    public static IQueryable<AuctionDto> ToAuctionDto(this IQueryable<Auction> auctions)
    {
        return auctions
            .Include(a => a.Images)
            .Select(auction => new AuctionDto(
                auction.Id,
                auction.Title,
                auction.Description,
                auction.StartDate,
                auction.EndDate,
                auction.StartingPrice,
                auction.CurrentPrice,
                auction.CreatedBy,
                auction.Images.Select(img => new AuctionImageDto(
                    img.Id,
                    img.AuctionId,
                    img.ImageUrl,
                    img.IsPrimary)).ToList()));
    }
}