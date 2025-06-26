using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BidNet.Features.Wallets.Hubs;

[Authorize]
public class WalletHub : Hub
{
    private static readonly Dictionary<string, string> ConnectionUserIds = new();
    private readonly ILogger<WalletHub> _logger;

    public WalletHub(ILogger<WalletHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal wallet group for targeted updates
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-wallet-{userId}");
            
            ConnectionUserIds[Context.ConnectionId] = userId;
            
            _logger.LogInformation("User {UserId} connected to WalletHub", userId);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionUserIds.TryGetValue(Context.ConnectionId, out var userId))
        {
            _logger.LogInformation("User {UserId} disconnected from WalletHub", userId);
            ConnectionUserIds.Remove(Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
