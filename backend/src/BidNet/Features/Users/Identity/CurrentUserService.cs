using BidNet.Domain.Entities;
using BidNet.Features.Common.Abstractions;
using System.Security.Claims;

namespace BidNet.Features.Users.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserId UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? new UserId(Guid.Parse(userId)) : throw new InvalidOperationException("User ID is not available.");
        }
    }

    public string UserName
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? throw new InvalidOperationException("User name is not available.");
        }
    }
}
