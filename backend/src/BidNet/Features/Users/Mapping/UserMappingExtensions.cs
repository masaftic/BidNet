using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Users.Mapping;

public static class UserMappingExtensions
{
    public static IQueryable<UserDto> ToUserDto(this IQueryable<User> query)
    {
        return query
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                EmailConfirmed = u.EmailConfirmed,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                CreatedDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                
                // Wallet will be loaded separately
                Balance = 0,
                HeldBalance = 0,
                Currency = "USD",
                
                // Activity stats
                TotalAuctions = 0,
                ActiveAuctions = 0,
                TotalBids = 0,
                WonAuctions = 0
            });
    }

    public static UserDto ToUserDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            CreatedDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow,
            
            // Wallet will be loaded separately
            Balance = 0,
            HeldBalance = 0,
            Currency = "USD",
            
            // Activity stats
            TotalAuctions = 0,
            ActiveAuctions = 0,
            TotalBids = 0,
            WonAuctions = 0
        };
    } 
}
