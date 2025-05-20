using Bidnet.Application.Common.Models;
using Bidnet.Domain.Entities;
using Bidnet.Domain.Enums;
using ErrorOr;

namespace Bidnet.Application.Common.Abstractions;

public interface IIdentityService
{
    Task<ErrorOr<User>> CreateUserAsync(string username, string email, string password, UserRole role = UserRole.Bidder);
    Task<User?> GetUserByEmailAsync(string email);
    Task<ErrorOr<AuthenticationResult>> AuthenticateAsync(string email, string password);
    Task<UserId?> GetUserIdAsync(string email);
    Task<UserRole?> GetUserRoleAsync(UserId userId);
    Task<bool> IsInRoleAsync(UserId userId, string role);
    Task<ErrorOr<Deleted>> DeleteUserAsync(UserId userId);
    Task<ErrorOr<AuthenticationResult>> RefreshTokenAsync(string token, string refreshToken);
}
