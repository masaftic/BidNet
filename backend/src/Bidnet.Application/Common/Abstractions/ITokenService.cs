using Bidnet.Application.Common.Models;
using Bidnet.Domain.Entities;
using Bidnet.Domain.Enums;
using System.Security.Claims;

namespace Bidnet.Application.Common.Abstractions;

public interface ITokenService
{
    string GenerateAccessToken(UserId userId, string email, UserRole role);
    string GenerateRefreshToken(UserId userId);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    string? GetUserIdFromToken(string token);
}
