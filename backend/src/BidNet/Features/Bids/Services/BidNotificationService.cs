using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Bids.Hubs;
using BidNet.Features.Bids.Mapping;
using BidNet.Features.Bids.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Bids.Services;

public interface IBidNotificationService
{
    Task NotifyBidPlaced(Bid bid);
    Task NotifyAuctionStatusChanged(Auction auction);
}

public class BidNotificationService : IBidNotificationService
{
    private readonly IHubContext<BidHub> _hubContext;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BidNotificationService> _logger;

    public BidNotificationService(
        IHubContext<BidHub> hubContext, 
        AppDbContext dbContext,
        ILogger<BidNotificationService> logger)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task NotifyBidPlaced(Bid bid)
    {
        try
        {
            // Fetch related entities to include in the notification
            var fullBid = await _dbContext.Bids
                .Include(b => b.User)
                .Include(b => b.Auction)
                .FirstOrDefaultAsync(b => b.Id == bid.Id);
            
            if (fullBid == null)
            {
                _logger.LogWarning("Bid not found for notification: {BidId}", bid.Id);
                return;
            }

            var bidResponse = fullBid.ToBidDto();
            
            // Add user name to the response
            bidResponse.UserName = fullBid.User.UserName;
            
            // Notify all clients in the auction group
            await _hubContext.Clients.Group(bid.AuctionId.Value.ToString())
                .SendAsync("ReceiveBid", bidResponse);
            
            // Create a more detailed auction update
            var auctionUpdated = new AuctionUpdatedDto
            {
                AuctionId = fullBid.AuctionId.Value,
                CurrentPrice = fullBid.Amount,
                LastBidTime = fullBid.CreatedAt,
                LastBidUserId = fullBid.UserId.Value,
                LastBidUserName = fullBid.User.UserName,
                BidCount = await _dbContext.Bids.CountAsync(b => b.AuctionId == fullBid.AuctionId)
            };
            
            // Notify all clients about the auction update
            await _hubContext.Clients.All
                .SendAsync("AuctionUpdated", auctionUpdated);
            
            _logger.LogInformation(
                "Bid notification sent for Auction {AuctionId}, Amount: {Amount}, User: {Username}", 
                fullBid.AuctionId.Value, 
                fullBid.Amount, 
                fullBid.User.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bid notification for bid {BidId}", bid.Id);
        }
    }

    public async Task NotifyAuctionStatusChanged(Auction auction)
    {
        try
        {
            await _hubContext.Clients.Group(auction.Id.Value.ToString())
                .SendAsync("AuctionStatusChanged", new
                {
                    AuctionId = auction.Id.Value,
                    Status = auction.Status.ToString(),
                    WinnerId = auction.WinnerId?.Value
                });
            
            _logger.LogInformation(
                "Auction status notification sent: Auction {AuctionId}, Status: {Status}", 
                auction.Id.Value, 
                auction.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction status notification for auction {AuctionId}", auction.Id);
        }
    }
}
