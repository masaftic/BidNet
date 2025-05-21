using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using System.Security.Claims;

namespace BidNet.Features.Auth.Abstractions;

public interface ITokenService
{
    string GenerateAccessToken(UserId userId, string email, UserRole role);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    string? GetUserIdFromToken(string token);
}
