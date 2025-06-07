using BidNet.Domain.Entities;
using BidNet.Features.Auth.Models;

namespace BidNet.Features.Users.Mapping;

public static class UserMappingExtensions
{
    public static IQueryable<UserDto> ToUserDto(this IQueryable<User> query)
    {
        return query.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Roles = u.Roles.ToArray(),
            CreatedAt = u.CreatedAt,
        });
    }

    public static UserDto ToUserDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        };
    } 
}
