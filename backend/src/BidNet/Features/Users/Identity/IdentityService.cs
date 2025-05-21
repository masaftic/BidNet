using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Common.Abstractions;
using BidNet.Features.Common.Models;
using BidNet.Features.Users.Abstractions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace BidNet.Features.Users.Identity;

public class IdentityService : IIdentityService
{
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _dbContext;

    public IdentityService(
        ITokenService tokenService,
        AppDbContext dbContext)
    {
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<AuthenticationResult>> AuthenticateAsync(string email, string password)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null)
        {
            return Error.NotFound(description: "User with this email does not exist.");
        }
        
        if (!BC.Verify(password, user.PasswordHash))
        {
            return Error.Unauthorized(description: "Invalid password.");
        }
        
        var token = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _dbContext.RefreshTokens.Add(new RefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(7)));
        await _dbContext.SaveChangesAsync();
        
        return new AuthenticationResult
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<ErrorOr<User>> CreateUserAsync(string username, string email, string password, UserRole role = UserRole.Bidder)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (existingUser != null)
        {
            return Error.Conflict(description: "User with this email already exists.");
        }
        
        var passwordHash = BC.HashPassword(password);
        
        var user = new User(username, email, passwordHash, role);
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        return user;
    }

    public async Task<ErrorOr<Deleted>> DeleteUserAsync(UserId userId)
    {
        var user = await _dbContext.Users.FindAsync(userId.Value);
        
        if (user == null)
        {
            return Error.NotFound(description: "User not found.");
        }
        
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        
        return Result.Deleted;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserId?> GetUserIdAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user?.Id;
    }

    public async Task<UserRole?> GetUserRoleAsync(UserId userId)
    {
        var user = await _dbContext.Users.FindAsync(userId.Value);
        return user?.Role;
    }

    public async Task<bool> IsInRoleAsync(UserId userId, string role)
    {
        var user = await _dbContext.Users.FindAsync(userId.Value);
        
        if (user == null)
        {
            return false;
        }
        
        return user.Role.ToString() == role;
    }

    public async Task<ErrorOr<AuthenticationResult>> RefreshTokenAsync(UserId userId, string refreshToken)
    {
        var user = await _dbContext.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return Error.NotFound(description: "User not found.");
        }

        var refreshTokenEntity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

        if (refreshTokenEntity == null || refreshTokenEntity.IsRevoked || refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            return Error.Unauthorized(description: "Invalid or expired refresh token.");
        }

        refreshTokenEntity.Revoke();

        var newAccessToken = _tokenService.GenerateAccessToken(userId, user.Email, user.Role);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        _dbContext.RefreshTokens.Add(new RefreshToken(userId, newRefreshToken, DateTime.UtcNow.AddDays(7)));

        await _dbContext.SaveChangesAsync();

        return new AuthenticationResult
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
