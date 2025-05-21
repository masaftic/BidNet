using AutoMapper;
using BidNet.Domain.Entities;
using BidNet.Features.Auth.Commands;
using BidNet.Features.Auth.Models;

namespace BidNet.Features.Auth.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, UserResponse>();
        CreateMap<RegisterCommand, User>();
    }
}
