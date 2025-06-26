using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Bids.Mapping;
using BidNet.Features.Bids.Models;
using BidNet.Features.Bids.Services;
using BidNet.Features.Wallets.Commands;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Bids.Commands;

public class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}

public record PlaceBidCommand(AuctionId AuctionId, decimal Amount) : IRequest<ErrorOr<BidDto>>;

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, ErrorOr<BidDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _userService;
    private readonly IBidNotificationService _bidNotificationService;
    private readonly IMediator _mediator;

    public PlaceBidCommandHandler(
        AppDbContext dbContext,
        ICurrentUserService userService,
        IBidNotificationService bidNotificationService,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _userService = userService;
        _bidNotificationService = bidNotificationService;
        _mediator = mediator;
    }

    public async Task<ErrorOr<BidDto>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions
            .Include(a => a.Bids)
            .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found");
        }

        // Find current winning bid if any
        var currentWinningBid = auction.Bids.FirstOrDefault(b => b.IsWinning);
        var currentWinner = currentWinningBid?.UserId;
        
        // Hold funds in wallet before placing bid
        var holdFundsCommand = new HoldFundsForBidCommand(request.AuctionId, request.Amount);
        var holdResult = await _mediator.Send(holdFundsCommand, cancellationToken);
        
        if (holdResult.IsError)
        {
            return Error.Validation(description: "Failed to hold funds: " + string.Join(", ", 
                holdResult.Errors.Select(e => e.Description)));
        }

        // Create new bid
        var bid = new Bid(auction.Id, _userService.UserId, request.Amount);
        var placeBidResult = auction.PlaceBid(bid);
        if (placeBidResult.IsError)
        {
            // If bid fails, we should release the held funds
            var releaseFundsCommand = new ReleaseBidFundsCommand(request.AuctionId, _userService.UserId, request.Amount);
            await _mediator.Send(releaseFundsCommand, cancellationToken);
            
            return placeBidResult.Errors;
        }

        // If there was a previous winning bid, release those funds
        if (currentWinner.HasValue && currentWinningBid != null)
        {
            var releaseFundsCommand = new ReleaseBidFundsCommand(
                request.AuctionId, 
                currentWinner.Value, 
                currentWinningBid.Amount);
                
            await _mediator.Send(releaseFundsCommand, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Send real-time notification
        await _bidNotificationService.NotifyBidPlaced(bid);

        return bid.ToBidDto();
    }
}
