using BidNet.Domain.Entities;
using System.Security.Claims;

namespace BidNet.Shared.Services;

public interface ICurrentUserService
{
    UserId UserId { get; }
    bool IsInRole(string role);
    string UserName { get; }
}

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

    public bool IsInRole(string role)
    {
        var user = ClaimsPrincipal.Current;
        return user?.IsInRole(role) ?? false;
    }

    public string UserName => httpContextAccessor.HttpContext?.User.Identity?.Name ?? throw new InvalidOperationException("User name is not available.");
}
