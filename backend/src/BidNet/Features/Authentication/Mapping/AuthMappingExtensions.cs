using BidNet.Domain.Entities;
using BidNet.Features.Authentication.Models;

namespace BidNet.Features.Authentication.Mapping;

public static class AuthMappingExtensions
{
    public static AuthResponse ToAuthResponse(this User user, IEnumerable<string> roles, string accessToken, string refreshToken, DateTime tokenExpiration)
    {
        return new AuthResponse
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiration = tokenExpiration
        };
    }
}
