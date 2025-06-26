using BidNet.Domain.Entities;
using BidNet.Features.Users.Models;

namespace BidNet.Features.Users.Mapping;

public static class UserMappingExtensions
{
    public static IQueryable<UserDto> ToUserDto(this IQueryable<User> query)
    {
        return query.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
        });
    }

    public static UserDto ToUserDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
        };
    } 
}
