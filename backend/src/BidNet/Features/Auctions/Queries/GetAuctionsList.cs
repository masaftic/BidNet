using BidNet.Data.Persistence;
using BidNet.Features.Auctions.Mapping;
using BidNet.Features.Auctions.Models;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Queries;

public record GetAuctionsListQuery : IRequest<ErrorOr<List<AuctionDto>>>;

public class GetAuctionsListQueryHandler : IRequestHandler<GetAuctionsListQuery, ErrorOr<List<AuctionDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetAuctionsListQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<List<AuctionDto>>> Handle(GetAuctionsListQuery request, CancellationToken cancellationToken)
    {
        var auctions = await _dbContext
                    .Auctions
                    .AsNoTracking()
                    .ToAuctionDto()
                    .ToListAsync(cancellationToken: cancellationToken);

        return auctions;
    }
}
