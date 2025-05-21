using Bidnet.Application.Common.Abstractions;
using Bidnet.Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bidnet.Application.Auctions.Queries;

public record GetAuctionsListQuery : IRequest<ErrorOr<List<Auction>>>;

public class GetAuctionsListQueryHandler : IRequestHandler<GetAuctionsListQuery, ErrorOr<List<Auction>>>
{
    private readonly IAppDbContext _dbContext;

    public GetAuctionsListQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<List<Auction>>> Handle(GetAuctionsListQuery request, CancellationToken cancellationToken)
    {
        var auctions = await _dbContext.Auctions.ToListAsync(cancellationToken);
        return auctions;
    }
}
