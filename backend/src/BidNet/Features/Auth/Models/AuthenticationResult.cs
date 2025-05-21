namespace BidNet.Features.Auth.Models;

public record AuthenticationResult
{
    public required string Token { get; init; }
    public required string RefreshToken { get; init; }
}
