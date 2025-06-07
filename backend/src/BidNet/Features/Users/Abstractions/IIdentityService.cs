using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Auth.Models;
using ErrorOr;

namespace BidNet.Features.Users.Abstractions;

public interface IIdentityService
{
    Task<ErrorOr<User>> CreateUserAsync(string username, string email, string password, UserRole role = UserRole.Bidder);
    Task<User?> GetUserByEmailAsync(string email);
    Task<ErrorOr<AuthenticationResult>> AuthenticateAsync(string email, string password);
    Task<UserId?> GetUserIdAsync(string email);
    Task<List<UserRole>> GetUserRolesAsync(UserId userId);
    Task<bool> IsInRoleAsync(UserId userId, string role);
    Task<ErrorOr<Deleted>> DeleteUserAsync(UserId userId);
    Task<ErrorOr<AuthenticationResult>> RefreshTokenAsync(UserId userId, string refreshToken);
}
