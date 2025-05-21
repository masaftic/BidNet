using BidNet.Domain.Enums;

namespace BidNet.Features.Users.Models;

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    UserRole Role,
    decimal Balance);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    UserRole Role = UserRole.Bidder);

public record LoginRequest(
    string Email,
    string Password);

public record LoginResponse(
    string Token,
    string RefreshToken,
    UserResponse User);

public record RefreshTokenRequest(
    string RefreshToken);
    
public record RefreshTokenResponse(
    string Token,
    string RefreshToken);
