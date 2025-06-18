using BidNet.Domain.Entities;
using BidNet.Features.Authentication.Models;
using ErrorOr;

namespace BidNet.Features.Authentication.Services;

public interface ITokenService
{
    Task<TokenDto> GenerateTokensAsync(User user, IEnumerable<string> roles);
    Task<ErrorOr<TokenDto>> RefreshTokenAsync(string refreshToken);
}
