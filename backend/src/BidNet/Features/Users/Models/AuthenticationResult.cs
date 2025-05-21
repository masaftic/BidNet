namespace BidNet.Features.Common.Models;

public record AuthenticationResult
{
    public required string Token { get; init; }
    public required string RefreshToken { get; init; }
}
