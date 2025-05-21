using AutoMapper;
using BidNet.Domain.Entities;
using BidNet.Features.Bids.Models;

namespace BidNet.Features.Bids.Mapping;

public class BidMappingProfile : Profile
{
    public BidMappingProfile()
    {
        CreateMap<Bid, BidResponse>();
    }
}
