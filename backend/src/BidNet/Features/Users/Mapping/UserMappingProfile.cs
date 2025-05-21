using AutoMapper;
using BidNet.Domain.Entities;
using BidNet.Features.Users.Models;

namespace BidNet.Features.Users.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserProfileResponse>();
        CreateMap<User, UserSummaryResponse>();
    }
}
