using BidNet.Domain.Entities;
using BidNet.Features.Bids.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BidNet.Features.Bids.Hubs;

[Authorize]
public class BidHub : Hub
{
    private static readonly Dictionary<string, HashSet<string>> AuctionConnections = new();
    private static readonly Dictionary<string, string> ConnectionUserNames = new();
    private readonly ILogger<BidHub> _logger;

    public BidHub(ILogger<BidHub> logger)
    {
        _logger = logger;
    }
    
    public async Task BroadcastBid(BidDto bid)
    {
        await Clients.All.SendAsync("ReceiveBid", bid);
    }

    public async Task JoinAuctionGroup(Guid auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
        
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        ConnectionUserNames[Context.ConnectionId] = userName;
        
        lock (AuctionConnections)
        {
            if (!AuctionConnections.ContainsKey(auctionId.ToString()))
            {
                AuctionConnections[auctionId.ToString()] = new HashSet<string>();
            }
            AuctionConnections[auctionId.ToString()].Add(Context.ConnectionId);
        }
        
        // Notify the group that someone joined
        await Clients.Group(auctionId.ToString())
            .SendAsync("UserJoinedAuction", new { Username = userName, ConnectionCount = GetConnectionCount(auctionId) });
        
        _logger.LogInformation("User {Username} joined auction {AuctionId}", userName, auctionId);
    }

    public async Task LeaveAuctionGroup(Guid auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
        
        var userName = ConnectionUserNames.GetValueOrDefault(Context.ConnectionId, "Anonymous");
        
        lock (AuctionConnections)
        {
            if (AuctionConnections.ContainsKey(auctionId.ToString()))
            {
                AuctionConnections[auctionId.ToString()].Remove(Context.ConnectionId);
            }
        }
        
        // Notify the group that someone left
        await Clients.Group(auctionId.ToString())
            .SendAsync("UserLeftAuction", new { Username = userName, ConnectionCount = GetConnectionCount(auctionId) });
        
        _logger.LogInformation("User {Username} left auction {AuctionId}", userName, auctionId);
    }
    
    public override async Task OnConnectedAsync()
    {
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        ConnectionUserNames[Context.ConnectionId] = userName;
        _logger.LogInformation("User {Username} connected with ID {ConnectionId}", userName, Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userName = ConnectionUserNames.GetValueOrDefault(Context.ConnectionId, "Anonymous");
        
        // Find and remove the connection from any auction groups
        List<string> auctionsToNotify = new();
        
        lock (AuctionConnections)
        {
            foreach (var auction in AuctionConnections)
            {
                if (auction.Value.Remove(Context.ConnectionId))
                {
                    auctionsToNotify.Add(auction.Key);
                }
            }
        }
        
        // Notify each group that the user disconnected
        foreach (var auctionId in auctionsToNotify)
        {
            await Clients.Group(auctionId).SendAsync(
                "UserLeftAuction",
                new { Username = userName, ConnectionCount = GetConnectionCount(Guid.Parse(auctionId)) });
        }
        
        ConnectionUserNames.Remove(Context.ConnectionId);
        _logger.LogInformation("User {Username} disconnected from hub", userName);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    private int GetConnectionCount(Guid auctionId)
    {
        lock (AuctionConnections)
        {
            if (AuctionConnections.TryGetValue(auctionId.ToString(), out var connections))
            {
                return connections.Count;
            }
            
            return 0;
        }
    }
}
