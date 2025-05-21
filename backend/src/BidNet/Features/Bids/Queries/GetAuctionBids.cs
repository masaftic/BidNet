using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
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
    private readonly AutoMapper.IMapper _mapper;

    public GetAuctionBidsQueryHandler(AppDbContext dbContext, AutoMapper.IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ErrorOr<AuctionBidsResponse>> Handle(GetAuctionBidsQuery request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found");
        }

        var bids = await _dbContext.Bids
            .Where(b => b.AuctionId == request.AuctionId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        var winningBid = bids.FirstOrDefault(b => b.IsWinning);

        return new AuctionBidsResponse
        {
            AuctionId = auction.Id,
            StartingPrice = auction.StartingPrice,
            CurrentPrice = auction.CurrentPrice,
            WinningBid = winningBid != null ? _mapper.Map<BidResponse>(winningBid) : null,
            BidHistory = _mapper.Map<IEnumerable<BidResponse>>(bids)
        };
    }
}
