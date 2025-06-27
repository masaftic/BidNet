using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Services;

public class AuctionSchedulerService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuctionSchedulerService> _logger;

    public AuctionSchedulerService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        AppDbContext dbContext,
        ILogger<AuctionSchedulerService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Schedule an auction to automatically start at its scheduled start time
    /// </summary>
    public void ScheduleAuctionStart(Auction auction)
    {
        if (auction.StartDate > DateTime.UtcNow)
        {
            _backgroundJobClient.Schedule(
                () => StartAuction(auction.Id),
                auction.StartDate - DateTime.UtcNow);
            
            _logger.LogInformation($"Scheduled auction {auction.Id} to start at {auction.StartDate}");
        }
        else
        {
            // If the start time is in the past, start immediately
            StartAuction(auction.Id).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Schedule all pending auctions to start at their scheduled start time
    /// </summary>
    public void ScheduleAllPendingAuctions()
    {
        // Run immediately to schedule all existing auctions
        CheckAndSchedulePendingAuctions().GetAwaiter().GetResult();

        // Setup recurring job to check for new auctions every minute
        _recurringJobManager.AddOrUpdate(
            "check-pending-auctions",
            () => CheckAndSchedulePendingAuctions(),
            Cron.Minutely());
        
        _logger.LogInformation("Set up recurring job to check for pending auctions every minute");
    }

    /// <summary>
    /// Start an auction by updating its status to Live
    /// </summary>
    public async Task StartAuction(AuctionId auctionId)
    {
        var auction = await _dbContext.Auctions.FirstAsync(a => a.Id == auctionId);
        if (auction.Status == AuctionStatus.Scheduled)
        {
            // Use the Start method on the Auction entity
            auction.Start();
            
            // Add an event log entry for the auction start
            var eventLog = new AuctionEventLog(
                auction.Id, 
                AuctionEventType.AuctionStarted, 
                "Auction automatically started by the system");
            
            _dbContext.AuctionEventLogs.Add(eventLog);
            
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Started auction {auctionId} at {DateTime.UtcNow}");
        }
    }

    /// <summary>
    /// Check for pending auctions that need to be scheduled
    /// </summary>
    public async Task CheckAndSchedulePendingAuctions()
    {
        var pendingAuctions = await _dbContext.Auctions
            .Where(a => a.Status == AuctionStatus.Scheduled && a.StartDate > DateTime.UtcNow)
            .ToListAsync();

        foreach (var auction in pendingAuctions)
        {
            // Check if this auction already has a scheduled job
            // If not, schedule it
            ScheduleAuctionStart(auction);
        }

        _logger.LogInformation($"Checked for pending auctions at {DateTime.UtcNow}, found {pendingAuctions.Count} pending auctions");
    }
}
