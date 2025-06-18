using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Authentication.Models;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BidNet.Features.Authentication.Services;

public class TokenService : ITokenService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly IOptions<JwtSettings> _jwtSettings;

    public TokenService(
        AppDbContext dbContext,
        UserManager<User> userManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _jwtSettings = jwtSettings;
    }

    public async Task<TokenDto> GenerateTokensAsync(User user, IEnumerable<string> roles)
    {
        // Generate access token
        var accessToken = GenerateAccessToken(user, roles);
        
        // Generate refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken(
            user.Id, 
            refreshToken, 
            DateTime.UtcNow.AddDays(7));
        
        // Store refresh token
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();
        
        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpirationInMinutes)
        };
    }

    public async Task<ErrorOr<TokenDto>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _dbContext.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            return Error.NotFound(description: "Refresh token not found");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Error.Validation(description: "Refresh token has expired");
        }

        if (storedToken.IsRevoked)
        {
            return Error.Validation(description: "Refresh token has been revoked");
        }

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);

        // Generate new access token
        var accessToken = GenerateAccessToken(user, roles);
        
        // Generate new refresh token
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken(
            user.Id, 
            refreshToken, 
            DateTime.UtcNow.AddDays(7));

        // Revoke old token
        storedToken.Revoke();
        
        // Store new refresh token
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();
        
        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpirationInMinutes)
        };
    }

    private string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpirationInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Value.Issuer,
            audience: _jwtSettings.Value.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
