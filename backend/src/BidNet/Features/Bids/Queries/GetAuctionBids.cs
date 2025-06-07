using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Bids.Mapping;
using BidNet.Features.Bids.Models;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Bids.Queries;

public class GetAuctionBidsQueryValidator : AbstractValidator<GetAuctionBidsQuery>
{
    public GetAuctionBidsQueryValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty();
    }
}

public record GetAuctionBidsQuery(AuctionId AuctionId) : IRequest<ErrorOr<AuctionBidsResponse>>;

public class GetAuctionBidsQueryHandler : IRequestHandler<GetAuctionBidsQuery, ErrorOr<AuctionBidsResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetAuctionBidsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<AuctionBidsResponse>> Handle(GetAuctionBidsQuery request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found");
        }

        var bids = await _dbContext.Bids
            .AsNoTracking()
            .Where(b => b.AuctionId == request.AuctionId)
            .OrderByDescending(b => b.CreatedAt)
            .ToBidDto()
            .ToListAsync(cancellationToken);

        var winningBid = bids.FirstOrDefault(b => b.IsWinning);

        return new AuctionBidsResponse
        {
            AuctionId = auction.Id,
            StartingPrice = auction.StartingPrice,
            CurrentPrice = auction.CurrentPrice,
            WinningBid = winningBid,
            BidHistory = bids
        };
    }
}
