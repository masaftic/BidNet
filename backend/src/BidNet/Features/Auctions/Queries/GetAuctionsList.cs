using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Queries;

public record GetAuctionsListQuery : IRequest<ErrorOr<List<Auction>>>;

public class GetAuctionsListQueryHandler : IRequestHandler<GetAuctionsListQuery, ErrorOr<List<Auction>>>
{
    private readonly AppDbContext _dbContext;

    public GetAuctionsListQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<List<Auction>>> Handle(GetAuctionsListQuery request, CancellationToken cancellationToken)
    {
        var auctions = await _dbContext.Auctions.ToListAsync(cancellationToken);
        return auctions;
    }
}
