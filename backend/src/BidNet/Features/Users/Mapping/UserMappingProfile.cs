using AutoMapper;
using BidNet.Domain.Entities;
using BidNet.Features.Users.Commands;
using BidNet.Features.Users.Models;

namespace BidNet.Features.Users.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponse>();
        CreateMap<RegisterCommand, User>();
    }
}
