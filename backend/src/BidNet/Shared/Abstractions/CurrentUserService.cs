using BidNet.Domain.Entities;
using System.Security.Claims;

namespace BidNet.Shared.Abstractions;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public UserId UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? new UserId(Guid.Parse(userId)) : throw new InvalidOperationException("User ID is not available.");
        }
    }

    public string UserName => httpContextAccessor.HttpContext?.User.Identity?.Name ?? throw new InvalidOperationException("User name is not available.");
}
