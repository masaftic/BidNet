using AutoMapper;
using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Commands;
using BidNet.Features.Auctions.Models;

namespace BidNet.Features.Auctions.Mapping;

public class AuctionMappingProfile : Profile
{
    public AuctionMappingProfile()
    {
        CreateMap<Auction, AuctionResponse>();
        CreateMap<CreateAuctionCommand, Auction>();
        CreateMap<UpdateAuctionCommand, Auction>();
    }
}
