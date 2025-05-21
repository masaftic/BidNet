using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Shared.Abstractions;
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

    public PlaceBidCommandHandler(AppDbContext dbContext, ICurrentUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<ErrorOr<Bid>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found");
        }

        // Check if auction is live
        if (auction.Status != AuctionStatus.Live)
        {
            return Error.Validation(description: "Auction is not live");
        }

        // Check if amount is higher than current price
        if (auction.CurrentPrice.HasValue && request.Amount <= auction.CurrentPrice.Value)
        {
            return Error.Validation(description: "Bid amount must be higher than current price");
        }

        // Check if amount is higher than starting price
        if (!auction.CurrentPrice.HasValue && request.Amount <= auction.StartingPrice)
        {
            return Error.Validation(description: "Bid amount must be higher than starting price");
        }

        // Create new bid
        var bid = new Bid(auction.Id, _userService.UserId, request.Amount);

        // Update previous winning bid
        var previousWinningBid = await _dbContext.Bids
            .FirstOrDefaultAsync(b => b.AuctionId == auction.Id && b.IsWinning, cancellationToken);

        previousWinningBid?.UpdateWinningStatus(false);

        // Set new bid as winning
        bid.UpdateWinningStatus(true);

        // Update auction's current price
        auction.UpdateCurrentPrice(request.Amount);

        // Add bid to database
        _dbContext.Bids.Add(bid);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return bid;
    }
}
