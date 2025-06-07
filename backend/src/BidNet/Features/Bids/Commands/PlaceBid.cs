using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Bids.Services;
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

public record PlaceBidCommand(AuctionId AuctionId, decimal Amount) : IRequest<ErrorOr<Bid>>;

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, ErrorOr<Bid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _userService;
    private readonly IBidNotificationService _bidNotificationService;

    public PlaceBidCommandHandler(
        AppDbContext dbContext,
        ICurrentUserService userService,
        IBidNotificationService bidNotificationService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _bidNotificationService = bidNotificationService;
    }

    public async Task<ErrorOr<Bid>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions
            .Include(a => a.Bids)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found");
        }

        // Create new bid
        var bid = new Bid(auction.Id, _userService.UserId, request.Amount);
        var placeBidResult = auction.PlaceBid(bid);
        if (placeBidResult.IsError)
            return placeBidResult.Errors;


        await _dbContext.SaveChangesAsync(cancellationToken);

        // Send real-time notification
        await _bidNotificationService.NotifyBidPlaced(bid);

        return bid;
    }
}
